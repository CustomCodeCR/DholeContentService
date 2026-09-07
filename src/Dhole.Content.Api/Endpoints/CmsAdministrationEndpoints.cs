using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using Dhole.Content.Api.Authorization;
using Dhole.Content.Application.Services;
using Dhole.Content.Contracts;

namespace Dhole.Content.Api.Endpoints;

public static class CmsAdministrationEndpoints
{
    public static IEndpointRouteBuilder MapCmsAdministrationEndpoints(this IEndpointRouteBuilder app)
    {
        var tax = app.MapGroup("/api/v1/taxonomies").WithTags("CMS Taxonomies");
        tax.MapGet("/", async (string? siteKey, string? kind, CmsAdministrationService service, CancellationToken ct)
            => Results.Ok(await service.TaxonomiesAsync(siteKey, kind, ct))).RequireScope(ContentScopes.View);
        tax.MapPost("/", async (TaxonomyWriteRequest request, CmsAdministrationService service, CancellationToken ct)
            => Results.Created("/api/v1/taxonomies", await service.CreateTaxonomyAsync(request, ct))).RequireScope(ContentScopes.Edit);

        var media = app.MapGroup("/api/v1/media").WithTags("CMS Media");
        media.MapGet("/", async (string? search, int? page, int? pageSize, CmsAdministrationService service, CancellationToken ct)
            => Results.Ok(await service.BrowseMediaAsync(search, page ?? 1, pageSize ?? 25, ct))).RequireScope(ContentScopes.View);

        media.MapPost("/register", async (MediaRegisterRequest request, HttpContext context, CmsAdministrationService service, CancellationToken ct)
            => Results.Created("/api/v1/media", await service.RegisterMediaAsync(request, UserId(context.User), ct)))
            .RequireScope(ContentScopes.MediaUpload);

        media.MapPost("/upload", async (
            HttpRequest request,
            HttpContext context,
            IHttpClientFactory clients,
            CmsAdministrationService service,
            CancellationToken ct) =>
        {
            if (!request.HasFormContentType)
                return Results.Problem("La carga debe ser multipart/form-data.", statusCode: 400);

            var form = await request.ReadFormAsync(ct);
            var file = form.Files.GetFile("file") ?? form.Files.FirstOrDefault();
            if (file is null) return Results.Problem("Debe adjuntar un archivo.", statusCode: 400);

            var entityId = Guid.NewGuid();
            using var content = new MultipartFormDataContent();
            await using var stream = file.OpenReadStream();
            var streamContent = new StreamContent(stream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType ?? "application/octet-stream");
            content.Add(streamContent, "file", file.FileName);
            content.Add(new StringContent("DholeContentService"), "sourceService");
            content.Add(new StringContent("CmsMedia"), "entityType");
            content.Add(new StringContent(entityId.ToString()), "entityId");
            var metadataJson = form["metadataJson"].ToString();
            if (!string.IsNullOrWhiteSpace(metadataJson)) content.Add(new StringContent(metadataJson), "metadataJson");

            var client = clients.CreateClient("DholeStorage");
            using var outbound = new HttpRequestMessage(HttpMethod.Post, "api/v1/storage/files") { Content = content };
            var authorization = context.Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrWhiteSpace(authorization))
                outbound.Headers.TryAddWithoutValidation("Authorization", authorization);

            using var response = await client.SendAsync(outbound, ct);
            var json = await response.Content.ReadAsStringAsync(ct);
            if (!response.IsSuccessStatusCode)
                return Results.Content(json, "application/problem+json", statusCode: (int)response.StatusCode);

            using var document = JsonDocument.Parse(json);
            if (!document.RootElement.TryGetProperty("id", out var idElement) || !Guid.TryParse(idElement.ToString(), out var storageFileId))
                return Results.Problem("Storage no devolvió un id de archivo válido.", statusCode: 502);

            var result = await service.RegisterMediaAsync(
                new MediaRegisterRequest(
                    storageFileId,
                    file.FileName,
                    file.ContentType ?? "application/octet-stream",
                    Null(form["altText"].ToString()),
                    Null(form["caption"].ToString()),
                    Null(metadataJson)),
                UserId(context.User),
                ct);

            return Results.Created($"/api/v1/media/{result.Id}", result);
        }).DisableAntiforgery().RequireScope(ContentScopes.MediaUpload);

        var menus = app.MapGroup("/api/v1/menus").WithTags("CMS Menus");
        menus.MapPut("/{location}", async (string location, MenuWriteRequest request, CmsAdministrationService service, CancellationToken ct) =>
        {
            var normalized = request with { Location = location };
            await service.ReplaceMenuAsync(normalized, ct);
            return Results.NoContent();
        }).RequireScope(ContentScopes.SettingsEdit);
        menus.MapGet("/{location}", async (string location, string? siteKey, CmsAdministrationService service, CancellationToken ct) =>
        {
            var result = await service.PublicMenuAsync(location, siteKey, ct);
            return result is null ? Results.NotFound() : Results.Ok(result);
        }).RequireScope(ContentScopes.View);

        var settings = app.MapGroup("/api/v1/settings").WithTags("CMS Settings");
        settings.MapGet("/", async (string? siteKey, CmsAdministrationService service, CancellationToken ct)
            => Results.Ok(await service.SettingsAsync(siteKey, false, ct))).RequireScope(ContentScopes.View);
        settings.MapPut("/{key}", async (string key, SiteSettingWriteRequest request, HttpContext context, CmsAdministrationService service, CancellationToken ct) =>
        {
            await service.UpsertSettingAsync(request with { Key = key }, UserId(context.User), ct);
            return Results.NoContent();
        }).RequireScope(ContentScopes.SettingsEdit);

        return app;
    }

    private static Guid? UserId(ClaimsPrincipal user)
    {
        var raw = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub") ?? user.FindFirstValue("user_id");
        return Guid.TryParse(raw, out var id) ? id : null;
    }

    private static string? Null(string value) => string.IsNullOrWhiteSpace(value) ? null : value;
}
