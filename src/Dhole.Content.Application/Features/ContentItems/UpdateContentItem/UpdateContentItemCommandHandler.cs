using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Content.Application.Abstractions.Auditing;
using Dhole.Content.Application.Abstractions.Cache;
using Dhole.Content.Application.Abstractions.Repositories;
using Dhole.Content.Application.Abstractions.Slugs;
using Dhole.Content.Application.Auditing;
using Dhole.Content.Domain.PageBuilder;
using Dhole.Content.Domain.Shared;

namespace Dhole.Content.Application.ContentItems.UpdateContentItem;

public sealed class UpdateContentItemCommandHandler(
    IContentItemRepository contents,
    ISlugGenerator slugs,
    IContentAuditService audit,
    IContentCacheService cache,
    IUnitOfWork unitOfWork
) : ICommandHandler<UpdateContentItemCommand, Result>
{
    public async Task<Result> HandleAsync(
        UpdateContentItemCommand command,
        CancellationToken cancellationToken = default
    )
    {
        var item = await contents.GetByIdWithDetailsAsync(command.Id, cancellationToken);
        if (item is null || item.IsDeleted)
        {
            return Result.Failure(ContentErrors.ContentNotFound);
        }

        string blocksJson;
        try
        {
            blocksJson = PageBuilderDocument.NormalizeAndValidate(command.BlocksJson);
        }
        catch (ArgumentException)
        {
            return Result.Failure(ContentErrors.InvalidBlocksJson);
        }

        var before = ContentAuditSnapshots.From(item);
        var oldSlug = item.Slug;
        item.CreateRevision(command.UpdatedBy, "before-update");

        var slug = string.IsNullOrWhiteSpace(command.Slug)
            ? item.Slug
            : slugs.Generate(command.Slug);
        var locale = string.IsNullOrWhiteSpace(command.Locale)
            ? item.Locale
            : command.Locale.Trim();

        if (await contents.ExistsBySlugAsync(item.SiteKey, locale, slug, item.Id, cancellationToken))
        {
            return Result.Failure(ContentErrors.ContentSlugAlreadyExists);
        }

        item.Update(
            command.Title,
            slug,
            command.Excerpt,
            blocksJson,
            command.RenderedHtml,
            command.FeaturedMediaId,
            command.SortOrder,
            command.IsFeatured,
            locale,
            command.UpdatedBy
        );

        item.ConfigureCmsMetadata(
            command.ParentContentId,
            command.TranslationGroupId,
            command.TemplateKey,
            command.UnpublishAtUtc,
            command.SitemapPriority,
            command.SitemapChangeFrequency,
            command.UpdatedBy
        );

        item.SetSeo(
            command.SeoTitle,
            command.SeoDescription,
            command.SeoKeywords,
            command.CanonicalUrl,
            command.Robots,
            command.OpenGraphMediaId,
            command.StructuredDataJson,
            command.UpdatedBy
        );

        if (command.TaxonomyTermIds is not null)
        {
            item.ReplaceTaxonomies(command.TaxonomyTermIds);
        }

        await audit.PublishAsync(
            new ContentAuditEvent(
                ContentAuditEventTypes.ContentUpdated,
                ContentAuditActions.Updated,
                ContentAuditEntityTypes.ContentItem,
                item.Id,
                command.UpdatedBy,
                Before: before,
                After: ContentAuditSnapshots.From(item)
            ),
            cancellationToken
        );

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await cache.RemoveContentAsync(item.SiteKey, oldSlug, cancellationToken);

        if (!string.Equals(oldSlug, item.Slug, StringComparison.OrdinalIgnoreCase))
        {
            await cache.RemoveContentAsync(item.SiteKey, item.Slug, cancellationToken);
        }

        return Result.Success();
    }
}
