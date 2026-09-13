using CustomCodeFramework.Cqrs.Dispatching;
using Dhole.Content.Api.Authorization;
using Dhole.Content.Api.Extensions;
using Dhole.Content.Api.Preview;
using Dhole.Content.Application.ContentItems.GetContentItemById;
using Dhole.Content.Domain.ContentItems.Enums;

namespace Dhole.Content.Api.Endpoints;

public static class PreviewEndpoints
{
    public static IEndpointRouteBuilder MapPreviewEndpoints(this IEndpointRouteBuilder app)
    {
        var admin = app.MapGroup("/api/content/items")
            .WithTags("Content Preview")
            .RequireAuthorization();

        admin.MapPost(
                "/{id:guid}/preview-token",
                async (
                    Guid id,
                    int? expiresInMinutes,
                    IQueryDispatcher dispatcher,
                    ContentPreviewTokenService tokens,
                    HttpContext context,
                    CancellationToken ct) =>
                {
                    var ttl = expiresInMinutes ?? ContentPreviewTokenService.DefaultExpirationMinutes;
                    if (ttl is < 1 or > ContentPreviewTokenService.MaxExpirationMinutes)
                    {
                        return Results.BadRequest(new
                        {
                            code = "preview.invalid_expiration",
                            message = $"expiresInMinutes debe estar entre 1 y {ContentPreviewTokenService.MaxExpirationMinutes}."
                        });
                    }

                    var content = await dispatcher.DispatchAsync(new GetContentItemByIdQuery(id), ct);
                    if (!content.IsSuccess)
                    {
                        return EndpointResults.FromResult(content, context);
                    }

                    if (!IsPreviewable(content.Value.Status))
                    {
                        return Results.BadRequest(new
                        {
                            code = "preview.content_not_previewable",
                            message = "El contenido archivado no puede previsualizarse."
                        });
                    }

                    var issued = tokens.Create(id, context.GetCurrentUserId(), ttl);
                    return EndpointResults.Ok(new PreviewTokenResponse(
                        issued.ContentId,
                        issued.Token,
                        issued.ExpiresAtUtc,
                        $"/api/public/preview/{issued.Token}"));
                })
            .RequireScope(ContentScopeNames.View);

        app.MapGet("/api/public/preview/{token}", GetPreviewAsync)
            .WithTags("Public Content Preview")
            .AllowAnonymous();

        // Temporary compatibility path while FennecWeb migrates fully to /api/public/*.
        app.MapGet("/api/content/public/preview/{token}", GetPreviewAsync)
            .WithTags("Public Content Preview (Legacy)")
            .AllowAnonymous();

        return app;
    }

    private static async Task<IResult> GetPreviewAsync(
        string token,
        ContentPreviewTokenService tokens,
        IQueryDispatcher dispatcher,
        HttpContext context,
        CancellationToken ct)
    {
        ApplyPrivatePreviewHeaders(context.Response);

        if (!tokens.TryValidate(token, out var preview))
        {
            return Results.NotFound();
        }

        var content = await dispatcher.DispatchAsync(new GetContentItemByIdQuery(preview.ContentId), ct);
        if (!content.IsSuccess || !IsPreviewable(content.Value.Status))
        {
            return Results.NotFound();
        }

        return EndpointResults.FromResult(content, context);
    }

    internal static bool IsPreviewable(string status)
        => !string.Equals(status, ContentStatus.Archived.ToString(), StringComparison.OrdinalIgnoreCase);

    private static void ApplyPrivatePreviewHeaders(HttpResponse response)
    {
        response.Headers.CacheControl = "no-store, no-cache, max-age=0, must-revalidate";
        response.Headers.Pragma = "no-cache";
        response.Headers.Expires = "0";
        response.Headers["X-Robots-Tag"] = "noindex, nofollow, noarchive";
        response.Headers["Referrer-Policy"] = "no-referrer";
    }

    public sealed record PreviewTokenResponse(
        Guid ContentId,
        string Token,
        DateTimeOffset ExpiresAtUtc,
        string ApiPath);
}
