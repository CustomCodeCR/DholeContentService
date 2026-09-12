using CustomCodeFramework.Cqrs.Dispatching;
using Dhole.Content.Api.Authorization;
using Dhole.Content.Api.Extensions;
using Dhole.Content.Application.Routes.CreateContentRoute;
using Dhole.Content.Application.Routes.GetContentRoutes;
using Dhole.Content.Application.Routes.UpdateContentRoute;
using Dhole.Content.Contracts.Routes;

namespace Dhole.Content.Api.Endpoints;

public static class ContentRouteEndpoints
{
    public static IEndpointRouteBuilder MapContentRouteEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/content/routes")
            .WithTags("Content Routes")
            .RequireAuthorization();

        group.MapGet(
                "/content/{contentId:guid}",
                async (Guid contentId, IQueryDispatcher dispatcher, CancellationToken ct) =>
                    EndpointResults.Ok(await dispatcher.DispatchAsync(new GetContentRoutesQuery(contentId), ct)))
            .RequireScope(ContentScopeNames.View);

        group.MapPost(
                "/",
                async (
                    CreateContentRouteRequest request,
                    ICommandDispatcher dispatcher,
                    HttpContext context,
                    CancellationToken ct
                ) => EndpointResults.FromResult(
                    await dispatcher.DispatchAsync(
                        new CreateContentRouteCommand(
                            request.SiteKey,
                            request.ContentId,
                            request.Locale,
                            request.Path,
                            request.IsPrimary,
                            request.IsActive,
                            context.GetCurrentUserId()),
                        ct),
                    context))
            .RequireScope(ContentScopeNames.Edit);

        group.MapPut(
                "/{id:guid}",
                async (
                    Guid id,
                    UpdateContentRouteRequest request,
                    ICommandDispatcher dispatcher,
                    HttpContext context,
                    CancellationToken ct
                ) => EndpointResults.FromResult(
                    await dispatcher.DispatchAsync(
                        new UpdateContentRouteCommand(
                            id,
                            request.SiteKey,
                            request.Locale,
                            request.Path,
                            request.IsPrimary,
                            request.IsActive,
                            context.GetCurrentUserId(),
                            request.CreatePermanentRedirect),
                        ct),
                    context))
            .RequireScope(ContentScopeNames.Edit);

        return app;
    }
}
