using CustomCodeFramework.Cqrs.Dispatching;
using Dhole.Content.Api.Authorization;
using Dhole.Content.Api.Extensions;
using Dhole.Content.Application.Sites.GetSiteByKey;
using Dhole.Content.Application.Sites.GetSites;
using Dhole.Content.Application.Sites.UpsertSite;
using Dhole.Content.Contracts.Sites;

namespace Dhole.Content.Api.Endpoints;

public static class SiteEndpoints
{
    public static IEndpointRouteBuilder MapSiteEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/content/sites")
            .WithTags("Content Sites")
            .RequireAuthorization();

        group.MapGet(
                "/",
                async (IQueryDispatcher dispatcher, CancellationToken cancellationToken) =>
                    EndpointResults.Ok(await dispatcher.DispatchAsync(new GetSitesQuery(), cancellationToken))
            )
            .RequireScope(ContentScopeNames.View);

        group.MapGet(
                "/{siteKey}",
                async (
                    string siteKey,
                    IQueryDispatcher dispatcher,
                    HttpContext context,
                    CancellationToken cancellationToken
                ) => EndpointResults.FromResult(
                    await dispatcher.DispatchAsync(new GetSiteByKeyQuery(siteKey), cancellationToken),
                    context
                )
            )
            .RequireScope(ContentScopeNames.View);

        group.MapPost(
                "/",
                async (
                    CreateSiteRequest request,
                    ICommandDispatcher dispatcher,
                    HttpContext context,
                    CancellationToken cancellationToken
                ) => EndpointResults.FromResult(
                    await dispatcher.DispatchAsync(
                        new UpsertSiteCommand(
                            request.SiteKey,
                            request.Name,
                            request.PrimaryDomain,
                            request.DefaultLocale,
                            request.TimeZone,
                            request.LogoMediaId,
                            request.FaviconMediaId,
                            request.DefaultOpenGraphMediaId,
                            request.Status,
                            context.GetCurrentUserId()
                        ),
                        cancellationToken
                    ),
                    context
                )
            )
            .RequireScope(ContentScopeNames.SettingsEdit);

        group.MapPut(
                "/{siteKey}",
                async (
                    string siteKey,
                    UpdateSiteRequest request,
                    ICommandDispatcher dispatcher,
                    HttpContext context,
                    CancellationToken cancellationToken
                ) => EndpointResults.FromResult(
                    await dispatcher.DispatchAsync(
                        new UpsertSiteCommand(
                            siteKey,
                            request.Name,
                            request.PrimaryDomain,
                            request.DefaultLocale,
                            request.TimeZone,
                            request.LogoMediaId,
                            request.FaviconMediaId,
                            request.DefaultOpenGraphMediaId,
                            request.Status,
                            context.GetCurrentUserId()
                        ),
                        cancellationToken
                    ),
                    context
                )
            )
            .RequireScope(ContentScopeNames.SettingsEdit);

        return app;
    }
}
