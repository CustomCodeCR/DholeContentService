using Dhole.Content.Application.Abstractions;
using Dhole.Content.Application.Services;
using Dhole.Content.Domain.Content;

namespace Dhole.Content.Api.Endpoints;

public static class PublicContentEndpoints
{
    public static IEndpointRouteBuilder MapPublicContentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/public").WithTags("Public CMS").AllowAnonymous();

        group.MapGet("/content/{slug}", async (
            string slug,
            string? siteKey,
            string? locale,
            IContentRepository repository,
            CancellationToken ct) =>
        {
            var item = await repository.GetPublishedBySlugAsync(slug.Trim('/').ToLowerInvariant(), Site(siteKey), locale, ct);
            return item is null ? Results.NotFound() : Results.Ok(ContentApplicationService.Map(item));
        });

        group.MapGet("/content", async (
            string? search,
            string? type,
            string? siteKey,
            string? locale,
            int? page,
            int? pageSize,
            IContentRepository repository,
            CancellationToken ct) =>
        {
            ContentType? parsed = null;
            if (!string.IsNullOrWhiteSpace(type) && Enum.TryParse<ContentType>(type, true, out var t)) parsed = t;
            var p = Math.Max(1, page ?? 1);
            var ps = Math.Clamp(pageSize ?? 12, 1, 50);
            var result = await repository.BrowseAsync(search, parsed, ContentStatus.Published, Site(siteKey), p, ps, ct);
            var items = result.Items
                .Where(x => locale == null || x.Locale == locale)
                .OrderByDescending(x => x.PublishedAtUtc)
                .Select(ContentApplicationService.Map)
                .ToArray();
            return Results.Ok(new { items, page = p, pageSize = ps, total = result.Total });
        });

        group.MapGet("/pages/{slug}", async (
            string slug,
            string? siteKey,
            string? locale,
            IContentRepository repository,
            CancellationToken ct) =>
        {
            var item = await repository.GetPublishedBySlugAsync(slug.Trim('/').ToLowerInvariant(), Site(siteKey), locale, ct);
            return item is null || item.Type != ContentType.Page
                ? Results.NotFound()
                : Results.Ok(ContentApplicationService.Map(item));
        });

        group.MapGet("/menus/{location}", async (
            string location,
            string? siteKey,
            CmsAdministrationService service,
            CancellationToken ct) =>
        {
            var result = await service.PublicMenuAsync(location, siteKey, ct);
            return result is null ? Results.NotFound() : Results.Ok(result);
        });

        group.MapGet("/settings", async (
            string? siteKey,
            CmsAdministrationService service,
            CancellationToken ct)
            => Results.Ok(await service.SettingsAsync(siteKey, true, ct)));

        return app;
    }

    private static string Site(string? value) => string.IsNullOrWhiteSpace(value) ? "main" : value.Trim();
}
