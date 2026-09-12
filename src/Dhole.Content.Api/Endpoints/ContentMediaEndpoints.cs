using CustomCodeFramework.Cqrs.Dispatching;
using Dhole.Content.Api.Authorization;
using Dhole.Content.Api.Extensions;
using Dhole.Content.Application.Media.AssociateContentMedia;
using Dhole.Content.Application.Media.GetContentMedia;
using Dhole.Content.Application.Media.RemoveContentMedia;
using Dhole.Content.Application.Media.UpdateContentMedia;
using Dhole.Content.Contracts.Media;
using Dhole.Content.Domain.Media;

namespace Dhole.Content.Api.Endpoints;

public static class ContentMediaEndpoints
{
    public static IEndpointRouteBuilder MapContentMediaEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/content/items/{contentId:guid}/media")
            .WithTags("Content Media Associations")
            .RequireAuthorization();

        group.MapGet(
                "/",
                async (Guid contentId, string? role, IQueryDispatcher dispatcher, CancellationToken cancellationToken) =>
                    EndpointResults.Ok(await dispatcher.DispatchAsync(
                        new GetContentMediaQuery(contentId, role),
                        cancellationToken)))
            .RequireScope(ContentScopeNames.View);

        group.MapGet("/roles", () => EndpointResults.Ok(ContentMediaRoles.All))
            .RequireScope(ContentScopeNames.View);

        group.MapPost(
                "/",
                async (
                    Guid contentId,
                    CreateContentMediaRequest request,
                    ICommandDispatcher dispatcher,
                    HttpContext context,
                    CancellationToken cancellationToken) =>
                    EndpointResults.FromResult(
                        await dispatcher.DispatchAsync(
                            new AssociateContentMediaCommand(
                                contentId,
                                request.MediaReferenceId,
                                request.Role,
                                request.SortOrder,
                                request.AltTextOverride,
                                request.CaptionOverride,
                                request.FocalX,
                                request.FocalY,
                                request.SettingsJson,
                                context.GetCurrentUserId()),
                            cancellationToken),
                        context))
            .RequireScope(ContentScopeNames.Edit);

        group.MapPut(
                "/{id:guid}",
                async (
                    Guid contentId,
                    Guid id,
                    UpdateContentMediaRequest request,
                    ICommandDispatcher dispatcher,
                    HttpContext context,
                    CancellationToken cancellationToken) =>
                    EndpointResults.FromResult(
                        await dispatcher.DispatchAsync(
                            new UpdateContentMediaCommand(
                                id,
                                request.Role,
                                request.SortOrder,
                                request.AltTextOverride,
                                request.CaptionOverride,
                                request.FocalX,
                                request.FocalY,
                                request.SettingsJson,
                                context.GetCurrentUserId()),
                            cancellationToken),
                        context))
            .RequireScope(ContentScopeNames.Edit);

        group.MapDelete(
                "/{id:guid}",
                async (
                    Guid contentId,
                    Guid id,
                    ICommandDispatcher dispatcher,
                    HttpContext context,
                    CancellationToken cancellationToken) =>
                    EndpointResults.FromResult(
                        await dispatcher.DispatchAsync(
                            new RemoveContentMediaCommand(id, context.GetCurrentUserId()),
                            cancellationToken),
                        context))
            .RequireScope(ContentScopeNames.Edit);

        return app;
    }
}
