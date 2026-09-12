using CustomCodeFramework.Cqrs.Dispatching;
using Dhole.Content.Api.Authorization;
using Dhole.Content.Api.Extensions;
using Dhole.Content.Application.Collections;
using Dhole.Content.Contracts.Collections;

namespace Dhole.Content.Api.Endpoints;

public static class CollectionEndpoints
{
    public static IEndpointRouteBuilder MapCollectionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/content/collections").WithTags("Content Collections").RequireAuthorization();

        group.MapGet("/", async (string? siteKey, IQueryDispatcher dispatcher, CancellationToken ct) =>
            EndpointResults.Ok(await dispatcher.DispatchAsync(new GetCollectionsQuery(siteKey), ct)))
            .RequireScope(ContentScopeNames.View);

        group.MapGet("/{id:guid}", async (Guid id, IQueryDispatcher dispatcher, HttpContext context, CancellationToken ct) =>
            EndpointResults.FromResult(await dispatcher.DispatchAsync(new GetCollectionByIdQuery(id), ct), context))
            .RequireScope(ContentScopeNames.View);

        group.MapPost("/", async (CreateCollectionRequest request, ICommandDispatcher dispatcher, HttpContext context, CancellationToken ct) =>
            EndpointResults.FromResult(await dispatcher.DispatchAsync(new CreateCollectionCommand(request.SiteKey, request.Code,
                request.Name, request.SettingsJson, request.IsActive, context.GetCurrentUserId()), ct), context))
            .RequireScope(ContentScopeNames.Edit);

        group.MapPut("/{id:guid}", async (Guid id, UpdateCollectionRequest request, ICommandDispatcher dispatcher, HttpContext context, CancellationToken ct) =>
            EndpointResults.FromResult(await dispatcher.DispatchAsync(new UpdateCollectionCommand(id, request.SiteKey, request.Code,
                request.Name, request.SettingsJson, request.IsActive, context.GetCurrentUserId()), ct), context))
            .RequireScope(ContentScopeNames.Edit);

        group.MapDelete("/{id:guid}", async (Guid id, ICommandDispatcher dispatcher, HttpContext context, CancellationToken ct) =>
            EndpointResults.FromResult(await dispatcher.DispatchAsync(new DeleteCollectionCommand(id, context.GetCurrentUserId()), ct), context))
            .RequireScope(ContentScopeNames.Edit);

        group.MapGet("/{collectionId:guid}/items", async (Guid collectionId, IQueryDispatcher dispatcher, HttpContext context, CancellationToken ct) =>
            EndpointResults.FromResult(await dispatcher.DispatchAsync(new GetCollectionItemsQuery(collectionId), ct), context))
            .RequireScope(ContentScopeNames.View);

        group.MapPost("/{collectionId:guid}/items", async (Guid collectionId, CreateCollectionItemRequest request, ICommandDispatcher dispatcher, HttpContext context, CancellationToken ct) =>
            EndpointResults.FromResult(await dispatcher.DispatchAsync(new CreateCollectionItemCommand(collectionId, request.DataJson,
                request.SortOrder, request.IsActive, context.GetCurrentUserId()), ct), context))
            .RequireScope(ContentScopeNames.Edit);

        group.MapPut("/{collectionId:guid}/items/{itemId:guid}", async (Guid collectionId, Guid itemId, UpdateCollectionItemRequest request, ICommandDispatcher dispatcher, HttpContext context, CancellationToken ct) =>
            EndpointResults.FromResult(await dispatcher.DispatchAsync(new UpdateCollectionItemCommand(collectionId, itemId,
                request.DataJson, request.SortOrder, request.IsActive, context.GetCurrentUserId()), ct), context))
            .RequireScope(ContentScopeNames.Edit);

        group.MapDelete("/{collectionId:guid}/items/{itemId:guid}", async (Guid collectionId, Guid itemId, ICommandDispatcher dispatcher, HttpContext context, CancellationToken ct) =>
            EndpointResults.FromResult(await dispatcher.DispatchAsync(new DeleteCollectionItemCommand(collectionId, itemId,
                context.GetCurrentUserId()), ct), context))
            .RequireScope(ContentScopeNames.Edit);

        return app;
    }
}
