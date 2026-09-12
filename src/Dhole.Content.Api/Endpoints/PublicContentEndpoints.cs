using CustomCodeFramework.Core.Pagination;
using CustomCodeFramework.Cqrs.Dispatching;
using Dhole.Content.Application.ContentItems.GetContentItems;
using Dhole.Content.Application.ContentItems.GetPublishedContentBySlug;
using Dhole.Content.Application.Navigation.GetNavigationMenu;
using Dhole.Content.Application.Routes.GetPublishedContentByRoute;
using Dhole.Content.Application.Settings.GetSiteSettings;
using Dhole.Content.Domain.ContentItems.Enums;

namespace Dhole.Content.Api.Endpoints;

public static class PublicContentEndpoints
{
    public static IEndpointRouteBuilder MapPublicContentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/content/public")
            .WithTags("Public Content")
            .AllowAnonymous();

        group.MapGet(
            "/resolve",
            async (
                string path,
                string? siteKey,
                string? locale,
                IQueryDispatcher dispatcher,
                HttpContext context,
                CancellationToken ct
            ) => EndpointResults.FromResult(
                await dispatcher.DispatchAsync(
                    new GetPublishedContentByRouteQuery(
                        siteKey ?? "main",
                        locale ?? "es-CR",
                        path),
                    ct),
                context));

        // Legacy slug endpoints remain available during the CMS migration.
        // New public page rendering should use /resolve so the canonical URL comes from content_routes.
        group.MapGet(
            "/content/{slug}",
            async (string slug, string? siteKey, string? locale, IQueryDispatcher dispatcher, HttpContext context, CancellationToken ct) =>
                EndpointResults.FromResult(
                    await dispatcher.DispatchAsync(new GetPublishedContentBySlugQuery(siteKey ?? "main", slug, locale), ct),
                    context));

        group.MapGet(
            "/content",
            async (
                int? pageNumber,
                int? pageSize,
                string? siteKey,
                string? type,
                string? search,
                string? locale,
                IQueryDispatcher dispatcher,
                CancellationToken ct
            ) =>
            {
                ContentType? parsedType = Enum.TryParse<ContentType>(type, true, out var typeValue) ? typeValue : null;
                var result = await dispatcher.DispatchAsync(
                    new GetContentItemsQuery(
                        PageRequest.Create(pageNumber ?? 1, pageSize ?? 20),
                        siteKey ?? "main",
                        parsedType,
                        ContentStatus.Published,
                        search,
                        locale),
                    ct);
                return EndpointResults.FromPaged(result);
            });

        group.MapGet(
            "/pages/{slug}",
            async (string slug, string? siteKey, string? locale, IQueryDispatcher dispatcher, HttpContext context, CancellationToken ct) =>
                EndpointResults.FromResult(
                    await dispatcher.DispatchAsync(new GetPublishedContentBySlugQuery(siteKey ?? "main", slug, locale), ct),
                    context));

        group.MapGet(
            "/menus/{location}",
            async (string location, string? siteKey, IQueryDispatcher dispatcher, HttpContext context, CancellationToken ct) =>
                EndpointResults.FromResult(
                    await dispatcher.DispatchAsync(new GetNavigationMenuQuery(siteKey ?? "main", location), ct),
                    context));

        group.MapGet(
            "/settings",
            async (string? siteKey, IQueryDispatcher dispatcher, CancellationToken ct) =>
                EndpointResults.Ok(await dispatcher.DispatchAsync(new GetSiteSettingsQuery(siteKey ?? "main", true), ct)));

        return app;
    }
}
