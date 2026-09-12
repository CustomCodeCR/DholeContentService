using System.Net.Http.Headers;
using System.Text.Json;
using CustomCodeFramework.Core.Pagination;
using CustomCodeFramework.Cqrs.Dispatching;
using Dhole.Content.Api.Authorization;
using Dhole.Content.Api.Extensions;
using Dhole.Content.Application.Media.DeleteMedia;
using Dhole.Content.Application.Media.GetMedia;
using Dhole.Content.Application.Media.RegisterMedia;
using Dhole.Content.Application.Media.UpdateMediaMetadata;
using Dhole.Content.Contracts.Media;

namespace Dhole.Content.Api.Endpoints;

public static class MediaEndpoints
{
    public static IEndpointRouteBuilder MapMediaEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/content/media")
            .WithTags("Content Media")
            .RequireAuthorization();

        group.MapGet(
                "/",
                async (
                    int? pageNumber,
                    int? pageSize,
                    string? search,
                    string? contentType,
                    IQueryDispatcher dispatcher,
                    CancellationToken cancellationToken
                ) => EndpointResults.FromPaged(
                    await dispatcher.DispatchAsync(
                        new GetMediaQuery(PageRequest.Create(pageNumber ?? 1, pageSize ?? 25), search, contentType),
                        cancellationToken
                    )
                )
            )
            .RequireScope(ContentScopeNames.View);

        group.MapPost(
                "/register",
                async (
                    RegisterMediaRequest request,
                    ICommandDispatcher dispatcher,
                    HttpContext context,
                    CancellationToken cancellationToken
                ) => EndpointResults.FromResult(
                    await dispatcher.DispatchAsync(
                        new RegisterMediaCommand(
                            request.StorageFileId,
                            request.FileName,
                            request.ContentType,
                            request.AltText,
                            request.Caption,
                            request.MetadataJson,
                            context.GetCurrentUserId()
                        ),
                        cancellationToken
                    ),
                    context
                )
            )
            .RequireScope(ContentScopeNames.MediaUpload);

        group.MapPatch(
                "/{id:guid}",
                async (
                    Guid id,
                    UpdateMediaMetadataRequest request,
                    ICommandDispatcher dispatcher,
                    HttpContext context,
                    CancellationToken cancellationToken
                ) => EndpointResults.FromResult(
                    await dispatcher.DispatchAsync(
                        new UpdateMediaMetadataCommand(
                            id,
                            request.AltText,
                            request.Caption,
                            request.MetadataJson,
                            context.GetCurrentUserId()
                        ),
                        cancellationToken
                    ),
                    context
                )
            )
            .RequireScope(ContentScopeNames.Edit);

        group.MapDelete(
                "/{id:guid}",
                async (
                    Guid id,
                    ICommandDispatcher dispatcher,
                    HttpContext context,
                    CancellationToken cancellationToken
                ) => EndpointResults.FromResult(
                    await dispatcher.DispatchAsync(
                        new DeleteMediaCommand(id, context.GetCurrentUserId()),
                        cancellationToken
                    ),
                    context
                )
            )
            .RequireScope(ContentScopeNames.MediaDelete);

        group.MapPost("/upload", UploadAsync)
            .DisableAntiforgery()
            .RequireScope(ContentScopeNames.MediaUpload);

        return app;
    }

    private static async Task<IResult> UploadAsync(
        HttpRequest request,
        HttpContext context,
        IHttpClientFactory clients,
        ICommandDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        if (!request.HasFormContentType)
            return Results.Problem("La carga debe ser multipart/form-data.", statusCode: 400);

        var form = await request.ReadFormAsync(cancellationToken);
        var file = form.Files.GetFile("file") ?? form.Files.FirstOrDefault();
        if (file is null)
            return Results.Problem("Debe adjuntar un archivo.", statusCode: 400);

        using var content = new MultipartFormDataContent();
        await using var stream = file.OpenReadStream();
        var streamContent = new StreamContent(stream);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue(
            string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType);
        content.Add(streamContent, "file", file.FileName);
        content.Add(new StringContent("DholeContentService"), "sourceService");
        content.Add(new StringContent("CmsMedia"), "entityType");
        content.Add(new StringContent(Guid.NewGuid().ToString()), "entityId");

        var metadata = form["metadataJson"].ToString();
        if (!string.IsNullOrWhiteSpace(metadata))
            content.Add(new StringContent(metadata), "metadataJson");

        var client = clients.CreateClient("DholeStorage");
        using var outbound = new HttpRequestMessage(HttpMethod.Post, "api/v1/storage/marketing/files")
        {
            Content = content
        };

        var auth = context.Request.Headers.Authorization.ToString();
        if (!string.IsNullOrWhiteSpace(auth))
            outbound.Headers.TryAddWithoutValidation("Authorization", auth);

        using var response = await client.SendAsync(outbound, cancellationToken);
        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
            return Results.Content(json, "application/problem+json", statusCode: (int)response.StatusCode);

        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        Guid storageId = default;

        if (root.TryGetProperty("file", out var marketingFile)
            && marketingFile.TryGetProperty("id", out var marketingFileId))
        {
            Guid.TryParse(marketingFileId.ToString(), out storageId);
        }

        if (storageId == Guid.Empty && root.TryGetProperty("id", out var direct))
            Guid.TryParse(direct.ToString(), out storageId);

        if (storageId == Guid.Empty
            && root.TryGetProperty("data", out var data)
            && data.TryGetProperty("id", out var nested))
        {
            Guid.TryParse(nested.ToString(), out storageId);
        }

        if (storageId == Guid.Empty)
            return Results.Problem("Storage no devolvió un id de archivo válido.", statusCode: 502);

        var metadataJson = root.TryGetProperty("file", out _)
            ? root.GetRawText()
            : Null(metadata);

        var result = await dispatcher.DispatchAsync(
            new RegisterMediaCommand(
                storageId,
                file.FileName,
                string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType,
                Null(form["altText"].ToString()),
                Null(form["caption"].ToString()),
                metadataJson,
                context.GetCurrentUserId()
            ),
            cancellationToken
        );

        return EndpointResults.FromResult(result, context);
    }

    private static string? Null(string value)
        => string.IsNullOrWhiteSpace(value) ? null : value;
}
