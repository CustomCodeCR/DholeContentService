using CustomCodeFramework.Cqrs.Dispatching;
using Dhole.Content.Api.Authorization;
using Dhole.Content.Api.Extensions;
using Dhole.Content.Application.Placements;
using Dhole.Content.Contracts.Placements;

namespace Dhole.Content.Api.Endpoints;

public static class PlacementEndpoints
{
    public static IEndpointRouteBuilder MapPlacementEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/content/placements").WithTags("Content Placements").RequireAuthorization();

        group.MapGet("/", async (string? siteKey, IQueryDispatcher dispatcher, CancellationToken ct) =>
            EndpointResults.Ok(await dispatcher.DispatchAsync(new GetPlacementsQuery(siteKey), ct)))
            .RequireScope(ContentScopeNames.View);

        group.MapGet("/{id:guid}", async (Guid id, IQueryDispatcher dispatcher, HttpContext context, CancellationToken ct) =>
            EndpointResults.FromResult(await dispatcher.DispatchAsync(new GetPlacementByIdQuery(id), ct), context))
            .RequireScope(ContentScopeNames.View);

        group.MapPost("/", async (CreatePlacementRequest request, ICommandDispatcher dispatcher, HttpContext context, CancellationToken ct) =>
            EndpointResults.FromResult(await dispatcher.DispatchAsync(new CreatePlacementCommand(request.SiteKey, request.Code,
                request.Name, request.AllowedTypesJson, request.MaxItems, request.SettingsJson, request.IsActive,
                context.GetCurrentUserId()), ct), context))
            .RequireScope(ContentScopeNames.BannersEdit);

        group.MapPut("/{id:guid}", async (Guid id, UpdatePlacementRequest request, ICommandDispatcher dispatcher, HttpContext context, CancellationToken ct) =>
            EndpointResults.FromResult(await dispatcher.DispatchAsync(new UpdatePlacementCommand(id, request.SiteKey, request.Code,
                request.Name, request.AllowedTypesJson, request.MaxItems, request.SettingsJson, request.IsActive,
                context.GetCurrentUserId()), ct), context))
            .RequireScope(ContentScopeNames.BannersEdit);

        group.MapDelete("/{id:guid}", async (Guid id, ICommandDispatcher dispatcher, HttpContext context, CancellationToken ct) =>
            EndpointResults.FromResult(await dispatcher.DispatchAsync(new DeletePlacementCommand(id, context.GetCurrentUserId()), ct), context))
            .RequireScope(ContentScopeNames.BannersEdit);

        group.MapGet("/{placementId:guid}/items", async (Guid placementId, IQueryDispatcher dispatcher, HttpContext context, CancellationToken ct) =>
            EndpointResults.FromResult(await dispatcher.DispatchAsync(new GetPlacementItemsQuery(placementId), ct), context))
            .RequireScope(ContentScopeNames.View);

        group.MapPost("/{placementId:guid}/items", async (Guid placementId, CreatePlacementItemRequest request, ICommandDispatcher dispatcher, HttpContext context, CancellationToken ct) =>
            EndpointResults.FromResult(await dispatcher.DispatchAsync(new CreatePlacementItemCommand(placementId, request.ContentId,
                request.SortOrder, request.ValidFromUtc, request.ValidToUtc, request.SettingsJson, request.IsActive,
                context.GetCurrentUserId()), ct), context))
            .RequireScope(ContentScopeNames.BannersEdit);

        group.MapPut("/{placementId:guid}/items/{itemId:guid}", async (Guid placementId, Guid itemId, UpdatePlacementItemRequest request, ICommandDispatcher dispatcher, HttpContext context, CancellationToken ct) =>
            EndpointResults.FromResult(await dispatcher.DispatchAsync(new UpdatePlacementItemCommand(placementId, itemId,
                request.SortOrder, request.ValidFromUtc, request.ValidToUtc, request.SettingsJson, request.IsActive,
                context.GetCurrentUserId()), ct), context))
            .RequireScope(ContentScopeNames.BannersEdit);

        group.MapDelete("/{placementId:guid}/items/{itemId:guid}", async (Guid placementId, Guid itemId, ICommandDispatcher dispatcher, HttpContext context, CancellationToken ct) =>
            EndpointResults.FromResult(await dispatcher.DispatchAsync(new DeletePlacementItemCommand(placementId, itemId,
                context.GetCurrentUserId()), ct), context))
            .RequireScope(ContentScopeNames.BannersEdit);

        return app;
    }
}
