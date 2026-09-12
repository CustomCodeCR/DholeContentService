using CustomCodeFramework.Core.Pagination;
using CustomCodeFramework.Cqrs.Dispatching;
using Dhole.Content.Api.Authorization;
using Dhole.Content.Api.Extensions;
using Dhole.Content.Application.ContentItems.ArchiveContent;
using Dhole.Content.Application.ContentItems.CreateContentItem;
using Dhole.Content.Application.ContentItems.DeleteContent;
using Dhole.Content.Application.ContentItems.GetContentItemById;
using Dhole.Content.Application.ContentItems.GetContentItems;
using Dhole.Content.Application.ContentItems.GetContentRevisions;
using Dhole.Content.Application.ContentItems.PublishContent;
using Dhole.Content.Application.ContentItems.RestoreContentRevision;
using Dhole.Content.Application.ContentItems.ScheduleContent;
using Dhole.Content.Application.ContentItems.SubmitContentForReview;
using Dhole.Content.Application.ContentItems.UnpublishContent;
using Dhole.Content.Application.ContentItems.UpdateContentItem;
using Dhole.Content.Contracts.ContentItems;
using Dhole.Content.Domain.ContentItems.Enums;

namespace Dhole.Content.Api.Endpoints;

public static class ContentItemEndpoints
{
    public static IEndpointRouteBuilder MapContentItemEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/content/items")
            .WithTags("Content Items")
            .RequireAuthorization();

        group.MapGet(
                "/",
                async (
                    int? pageNumber,
                    int? pageSize,
                    string? siteKey,
                    string? type,
                    string? status,
                    string? search,
                    string? locale,
                    IQueryDispatcher dispatcher,
                    CancellationToken cancellationToken
                ) =>
                {
                    ContentType? parsedType = Enum.TryParse<ContentType>(type, true, out var typeValue)
                        ? typeValue
                        : null;
                    ContentStatus? parsedStatus = Enum.TryParse<ContentStatus>(status, true, out var statusValue)
                        ? statusValue
                        : null;

                    var result = await dispatcher.DispatchAsync(
                        new GetContentItemsQuery(
                            PageRequest.Create(pageNumber ?? 1, pageSize ?? 25),
                            siteKey,
                            parsedType,
                            parsedStatus,
                            search,
                            locale
                        ),
                        cancellationToken
                    );

                    return EndpointResults.FromPaged(result);
                }
            )
            .RequireScope(ContentScopeNames.View);

        group.MapGet(
                "/{id:guid}",
                async (
                    Guid id,
                    IQueryDispatcher dispatcher,
                    HttpContext context,
                    CancellationToken cancellationToken
                ) => EndpointResults.FromResult(
                    await dispatcher.DispatchAsync(new GetContentItemByIdQuery(id), cancellationToken),
                    context
                )
            )
            .RequireScope(ContentScopeNames.View);

        group.MapPost(
                "/",
                async (
                    CreateContentItemRequest request,
                    ICommandDispatcher dispatcher,
                    HttpContext context,
                    CancellationToken cancellationToken
                ) =>
                {
                    var seo = request.Seo;
                    var result = await dispatcher.DispatchAsync(
                        new CreateContentItemCommand(
                            request.Type,
                            request.Title,
                            request.Slug,
                            request.Excerpt,
                            request.BlocksJson ?? "[]",
                            request.RenderedHtml,
                            request.FeaturedMediaId,
                            request.AuthorUserId,
                            request.Locale,
                            request.SortOrder,
                            request.IsFeatured,
                            request.TaxonomyTermIds ?? [],
                            seo?.Title,
                            seo?.Description,
                            seo?.Keywords,
                            seo?.CanonicalUrl,
                            seo?.Robots,
                            seo?.OpenGraphMediaId,
                            seo?.StructuredDataJson,
                            request.SiteKey,
                            request.ParentContentId,
                            request.TranslationGroupId,
                            request.TemplateKey,
                            request.UnpublishAtUtc,
                            request.SitemapPriority,
                            request.SitemapChangeFrequency,
                            context.GetCurrentUserId()
                        ),
                        cancellationToken
                    );
                    return EndpointResults.FromResult(result, context);
                }
            )
            .RequireScope(ContentScopeNames.Create);

        group.MapPut(
                "/{id:guid}",
                async (
                    Guid id,
                    UpdateContentItemRequest request,
                    ICommandDispatcher dispatcher,
                    HttpContext context,
                    CancellationToken cancellationToken
                ) =>
                {
                    var seo = request.Seo;
                    return EndpointResults.FromResult(
                        await dispatcher.DispatchAsync(
                            new UpdateContentItemCommand(
                                id,
                                request.Title,
                                request.Slug,
                                request.Excerpt,
                                request.BlocksJson ?? "[]",
                                request.RenderedHtml,
                                request.FeaturedMediaId,
                                request.Locale,
                                request.SortOrder,
                                request.IsFeatured,
                                request.TaxonomyTermIds,
                                seo?.Title,
                                seo?.Description,
                                seo?.Keywords,
                                seo?.CanonicalUrl,
                                seo?.Robots,
                                seo?.OpenGraphMediaId,
                                seo?.StructuredDataJson,
                                request.ParentContentId,
                                request.TranslationGroupId,
                                request.TemplateKey,
                                request.UnpublishAtUtc,
                                request.SitemapPriority,
                                request.SitemapChangeFrequency,
                                context.GetCurrentUserId()
                            ),
                            cancellationToken
                        ),
                        context
                    );
                }
            )
            .RequireScope(ContentScopeNames.Edit);

        group.MapPost(
                "/{id:guid}/review",
                async (Guid id, ICommandDispatcher dispatcher, HttpContext context, CancellationToken cancellationToken) =>
                    EndpointResults.FromResult(
                        await dispatcher.DispatchAsync(
                            new SubmitContentForReviewCommand(id, context.GetCurrentUserId()),
                            cancellationToken
                        ),
                        context
                    )
            )
            .RequireScope(ContentScopeNames.Edit);

        group.MapPost(
                "/{id:guid}/publish",
                async (Guid id, ICommandDispatcher dispatcher, HttpContext context, CancellationToken cancellationToken) =>
                    EndpointResults.FromResult(
                        await dispatcher.DispatchAsync(
                            new PublishContentCommand(id, context.GetCurrentUserId()),
                            cancellationToken
                        ),
                        context
                    )
            )
            .RequireScope(ContentScopeNames.Publish);

        group.MapPost(
                "/{id:guid}/schedule",
                async (
                    Guid id,
                    ScheduleContentRequest request,
                    ICommandDispatcher dispatcher,
                    HttpContext context,
                    CancellationToken cancellationToken
                ) => EndpointResults.FromResult(
                    await dispatcher.DispatchAsync(
                        new ScheduleContentCommand(id, request.ScheduledAtUtc, context.GetCurrentUserId()),
                        cancellationToken
                    ),
                    context
                )
            )
            .RequireScope(ContentScopeNames.Publish);

        group.MapPost(
                "/{id:guid}/unpublish",
                async (Guid id, ICommandDispatcher dispatcher, HttpContext context, CancellationToken cancellationToken) =>
                    EndpointResults.FromResult(
                        await dispatcher.DispatchAsync(
                            new UnpublishContentCommand(id, context.GetCurrentUserId()),
                            cancellationToken
                        ),
                        context
                    )
            )
            .RequireScope(ContentScopeNames.Publish);

        group.MapPost(
                "/{id:guid}/archive",
                async (Guid id, ICommandDispatcher dispatcher, HttpContext context, CancellationToken cancellationToken) =>
                    EndpointResults.FromResult(
                        await dispatcher.DispatchAsync(
                            new ArchiveContentCommand(id, context.GetCurrentUserId()),
                            cancellationToken
                        ),
                        context
                    )
            )
            .RequireScope(ContentScopeNames.Edit);

        group.MapDelete(
                "/{id:guid}",
                async (Guid id, ICommandDispatcher dispatcher, HttpContext context, CancellationToken cancellationToken) =>
                    EndpointResults.FromResult(
                        await dispatcher.DispatchAsync(
                            new DeleteContentCommand(id, context.GetCurrentUserId()),
                            cancellationToken
                        ),
                        context
                    )
            )
            .RequireScope(ContentScopeNames.Delete);

        group.MapGet(
                "/{id:guid}/revisions",
                async (Guid id, IQueryDispatcher dispatcher, CancellationToken cancellationToken) =>
                    EndpointResults.Ok(
                        await dispatcher.DispatchAsync(new GetContentRevisionsQuery(id), cancellationToken)
                    )
            )
            .RequireScope(ContentScopeNames.View);

        group.MapPost(
                "/{id:guid}/revisions/{revisionId:guid}/restore",
                async (
                    Guid id,
                    Guid revisionId,
                    RestoreContentRevisionRequest request,
                    ICommandDispatcher dispatcher,
                    HttpContext context,
                    CancellationToken cancellationToken
                ) => EndpointResults.FromResult(
                    await dispatcher.DispatchAsync(
                        new RestoreContentRevisionCommand(
                            id,
                            revisionId,
                            request.Reason,
                            context.GetCurrentUserId()
                        ),
                        cancellationToken
                    ),
                    context
                )
            )
            .RequireScope(ContentScopeNames.Edit);

        return app;
    }
}
