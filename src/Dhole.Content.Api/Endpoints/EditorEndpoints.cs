using CustomCodeFramework.Core.Pagination;
using CustomCodeFramework.Cqrs.Dispatching;
using Dhole.Content.Api.Authorization;
using Dhole.Content.Api.Extensions;
using Dhole.Content.Application.ContentItems.ArchiveContent;
using Dhole.Content.Application.ContentItems.CreateContentItem;
using Dhole.Content.Application.ContentItems.DeleteContent;
using Dhole.Content.Application.ContentItems.GetContentItemById;
using Dhole.Content.Application.ContentItems.GetContentItems;
using Dhole.Content.Application.ContentItems.PublishContent;
using Dhole.Content.Application.ContentItems.ScheduleContent;
using Dhole.Content.Application.ContentItems.SubmitContentForReview;
using Dhole.Content.Application.ContentItems.UnpublishContent;
using Dhole.Content.Application.ContentItems.UpdateContentItem;
using Dhole.Content.Application.Media.GetMedia;
using Dhole.Content.Contracts.ContentItems;
using Dhole.Content.Contracts.Editor;
using Dhole.Content.Domain.ContentItems.Enums;

namespace Dhole.Content.Api.Endpoints;

public static class EditorEndpoints
{
    public static IEndpointRouteBuilder MapEditorEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/content/editor")
            .WithTags("Simple Content Editor")
            .RequireAuthorization();

        group.MapGet(
                "/options",
                () => EndpointResults.Ok(
                    new EditorOptionsDto(
                        [
                            new("Page", "Página"),
                            new("News", "Noticia"),
                            new("Banner", "Banner"),
                            new("Announcement", "Anuncio"),
                            new("Video", "Video"),
                        ],
                        [
                            new("Draft", "Borrador"),
                            new("PendingReview", "Pendiente de aprobación"),
                            new("Scheduled", "Programado"),
                            new("Published", "Publicado"),
                            new("Archived", "Archivado"),
                        ],
                        "es-CR",
                        "main"
                    )
                )
            )
            .RequireScope(ContentScopeNames.View);

        group.MapGet(
                "/dashboard",
                async (
                    string? siteKey,
                    IQueryDispatcher dispatcher,
                    CancellationToken cancellationToken
                ) =>
                {
                    var page = PageRequest.Create(1, 1);
                    var resolvedSiteKey = string.IsNullOrWhiteSpace(siteKey) ? "main" : siteKey.Trim();

                    async Task<int> CountAsync(ContentType? type = null, ContentStatus? status = null)
                    {
                        var result = await dispatcher.DispatchAsync(
                            new GetContentItemsQuery(
                                page,
                                resolvedSiteKey,
                                type,
                                status,
                                null,
                                null
                            ),
                            cancellationToken
                        );
                        return result.TotalCount;
                    }

                    var pages = await CountAsync(ContentType.Page);
                    var news = await CountAsync(ContentType.News);
                    var banners = await CountAsync(ContentType.Banner);
                    var drafts = await CountAsync(status: ContentStatus.Draft);
                    var pending = await CountAsync(status: ContentStatus.PendingReview);
                    var scheduled = await CountAsync(status: ContentStatus.Scheduled);
                    var published = await CountAsync(status: ContentStatus.Published);
                    var media = await dispatcher.DispatchAsync(
                        new GetMediaQuery(page, null, null),
                        cancellationToken
                    );

                    return EndpointResults.Ok(
                        new EditorDashboardDto(
                            pages,
                            news,
                            banners,
                            media.TotalCount,
                            drafts,
                            pending,
                            scheduled,
                            published
                        )
                    );
                }
            )
            .RequireScope(ContentScopeNames.View);

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
                    ContentType? parsedType = null;
                    if (!string.IsNullOrWhiteSpace(type))
                    {
                        if (!Enum.TryParse<ContentType>(type, true, out var typeValue))
                        {
                            return Results.BadRequest(new { message = "Tipo de contenido inválido." });
                        }
                        parsedType = typeValue;
                    }

                    ContentStatus? parsedStatus = null;
                    if (!string.IsNullOrWhiteSpace(status))
                    {
                        if (!Enum.TryParse<ContentStatus>(status, true, out var statusValue))
                        {
                            return Results.BadRequest(new { message = "Estado de contenido inválido." });
                        }
                        parsedStatus = statusValue;
                    }

                    var result = await dispatcher.DispatchAsync(
                        new GetContentItemsQuery(
                            PageRequest.Create(pageNumber ?? 1, pageSize ?? 25),
                            string.IsNullOrWhiteSpace(siteKey) ? "main" : siteKey.Trim(),
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
                    await dispatcher.DispatchAsync(
                        new GetContentItemByIdQuery(id),
                        cancellationToken
                    ),
                    context
                )
            )
            .RequireScope(ContentScopeNames.View);

        group.MapPost(
                "/",
                async (
                    EditorContentRequest request,
                    ICommandDispatcher dispatcher,
                    HttpContext context,
                    CancellationToken cancellationToken
                ) =>
                {
                    if (!Enum.TryParse<ContentType>(request.Type, true, out var type))
                    {
                        return Results.BadRequest(new { message = "Tipo de contenido inválido." });
                    }

                    var seo = request.Seo;
                    var result = await dispatcher.DispatchAsync(
                        new CreateContentItemCommand(
                            type.ToString(),
                            request.Title,
                            request.Slug,
                            request.Excerpt,
                            "[]",
                            request.ContentHtml,
                            request.FeaturedMediaId,
                            null,
                            string.IsNullOrWhiteSpace(request.Locale) ? "es-CR" : request.Locale,
                            request.SortOrder ?? 0,
                            request.IsFeatured ?? false,
                            request.CategoryIds ?? [],
                            seo?.Title,
                            seo?.Description,
                            seo?.Keywords,
                            seo?.CanonicalUrl,
                            "index,follow",
                            seo?.OpenGraphMediaId,
                            null,
                            string.IsNullOrWhiteSpace(request.SiteKey) ? "main" : request.SiteKey,
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
                    EditorContentRequest request,
                    ICommandDispatcher dispatcher,
                    HttpContext context,
                    CancellationToken cancellationToken
                ) =>
                {
                    var seo = request.Seo;
                    var result = await dispatcher.DispatchAsync(
                        new UpdateContentItemCommand(
                            id,
                            request.Title,
                            request.Slug,
                            request.Excerpt,
                            "[]",
                            request.ContentHtml,
                            request.FeaturedMediaId,
                            string.IsNullOrWhiteSpace(request.Locale) ? "es-CR" : request.Locale,
                            request.SortOrder ?? 0,
                            request.IsFeatured ?? false,
                            request.CategoryIds ?? [],
                            seo?.Title,
                            seo?.Description,
                            seo?.Keywords,
                            seo?.CanonicalUrl,
                            "index,follow",
                            seo?.OpenGraphMediaId,
                            null,
                            context.GetCurrentUserId()
                        ),
                        cancellationToken
                    );

                    return EndpointResults.FromResult(result, context);
                }
            )
            .RequireScope(ContentScopeNames.Edit);

        group.MapPost(
                "/{id:guid}/submit",
                async (
                    Guid id,
                    ICommandDispatcher dispatcher,
                    HttpContext context,
                    CancellationToken cancellationToken
                ) => EndpointResults.FromResult(
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
                async (
                    Guid id,
                    ICommandDispatcher dispatcher,
                    HttpContext context,
                    CancellationToken cancellationToken
                ) => EndpointResults.FromResult(
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
                        new ScheduleContentCommand(
                            id,
                            request.ScheduledAtUtc,
                            context.GetCurrentUserId()
                        ),
                        cancellationToken
                    ),
                    context
                )
            )
            .RequireScope(ContentScopeNames.Publish);

        group.MapPost(
                "/{id:guid}/unpublish",
                async (
                    Guid id,
                    ICommandDispatcher dispatcher,
                    HttpContext context,
                    CancellationToken cancellationToken
                ) => EndpointResults.FromResult(
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
                async (
                    Guid id,
                    ICommandDispatcher dispatcher,
                    HttpContext context,
                    CancellationToken cancellationToken
                ) => EndpointResults.FromResult(
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
                async (
                    Guid id,
                    ICommandDispatcher dispatcher,
                    HttpContext context,
                    CancellationToken cancellationToken
                ) => EndpointResults.FromResult(
                    await dispatcher.DispatchAsync(
                        new DeleteContentCommand(id, context.GetCurrentUserId()),
                        cancellationToken
                    ),
                    context
                )
            )
            .RequireScope(ContentScopeNames.Delete);

        return app;
    }
}
