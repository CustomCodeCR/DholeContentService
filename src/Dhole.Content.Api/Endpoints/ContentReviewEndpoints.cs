using CustomCodeFramework.Cqrs.Dispatching;
using Dhole.Content.Api.Authorization;
using Dhole.Content.Api.Extensions;
using Dhole.Content.Application.Reviews;
using Dhole.Content.Contracts.Reviews;
using Dhole.Content.Domain.Reviews.Enums;

namespace Dhole.Content.Api.Endpoints;

public static class ContentReviewEndpoints
{
    public static IEndpointRouteBuilder MapContentReviewEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/content/reviews")
            .WithTags("Content Reviews")
            .RequireAuthorization();

        group.MapGet(
                "/",
                async (
                    Guid? contentId,
                    string? status,
                    IQueryDispatcher dispatcher,
                    CancellationToken cancellationToken
                ) =>
                {
                    ContentReviewStatus? parsedStatus = Enum.TryParse<ContentReviewStatus>(status, true, out var value)
                        ? value
                        : null;
                    return Results.Ok(await dispatcher.DispatchAsync(
                        new GetContentReviewsQuery(contentId, parsedStatus), cancellationToken));
                })
            .RequireScope(ContentScopeNames.View);

        group.MapGet(
                "/{id:guid}",
                async (Guid id, IQueryDispatcher dispatcher, HttpContext context, CancellationToken cancellationToken) =>
                    EndpointResults.FromResult(
                        await dispatcher.DispatchAsync(new GetContentReviewByIdQuery(id), cancellationToken), context))
            .RequireScope(ContentScopeNames.View);

        group.MapPost(
                "/{id:guid}/approve",
                async (
                    Guid id,
                    DecideContentReviewRequest request,
                    ICommandDispatcher dispatcher,
                    HttpContext context,
                    CancellationToken cancellationToken
                ) => EndpointResults.FromResult(
                    await dispatcher.DispatchAsync(
                        new ApproveContentReviewCommand(id, request.Comment, context.GetCurrentUserId()),
                        cancellationToken), context))
            .RequireScope(ContentScopeNames.Publish);

        group.MapPost(
                "/{id:guid}/reject",
                async (
                    Guid id,
                    DecideContentReviewRequest request,
                    ICommandDispatcher dispatcher,
                    HttpContext context,
                    CancellationToken cancellationToken
                ) => EndpointResults.FromResult(
                    await dispatcher.DispatchAsync(
                        new RejectContentReviewCommand(id, request.Comment, context.GetCurrentUserId()),
                        cancellationToken), context))
            .RequireScope(ContentScopeNames.Publish);

        return app;
    }
}
