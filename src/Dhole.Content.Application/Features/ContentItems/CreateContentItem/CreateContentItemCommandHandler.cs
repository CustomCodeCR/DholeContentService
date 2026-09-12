using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Content.Application.Abstractions.Auditing;
using Dhole.Content.Application.Abstractions.Cache;
using Dhole.Content.Application.Abstractions.Repositories;
using Dhole.Content.Application.Abstractions.Slugs;
using Dhole.Content.Application.Auditing;
using Dhole.Content.Domain.ContentItems.Entities;
using Dhole.Content.Domain.ContentItems.Enums;
using Dhole.Content.Domain.PageBuilder;
using Dhole.Content.Domain.Seo;
using Dhole.Content.Domain.Shared;

namespace Dhole.Content.Application.ContentItems.CreateContentItem;

public sealed class CreateContentItemCommandHandler(
    IContentItemRepository contents,
    ISlugGenerator slugs,
    IContentAuditService audit,
    IContentCacheService cache,
    IUnitOfWork unitOfWork
) : ICommandHandler<CreateContentItemCommand, Result<Guid>>
{
    public async Task<Result<Guid>> HandleAsync(CreateContentItemCommand c, CancellationToken ct = default)
    {
        if (!Enum.TryParse<ContentType>(c.Type, true, out var type))
            return Result.Failure<Guid>(ContentErrors.InvalidContentType);

        var blocksJson = string.IsNullOrWhiteSpace(c.BlocksJson) ? "[]" : c.BlocksJson;
        if (type == ContentType.Page)
        {
            try { blocksJson = PageBuilderDocument.NormalizeAndValidate(blocksJson); }
            catch (ArgumentException) { return Result.Failure<Guid>(ContentErrors.InvalidBlocksJson); }
        }

        NormalizedSeo seo;
        try
        {
            seo = SeoRules.Normalize(c.SeoTitle, c.SeoDescription, c.SeoKeywords, c.CanonicalUrl,
                c.Robots, c.OpenGraphMediaId, c.StructuredDataJson);
        }
        catch (ArgumentException)
        {
            return Result.Failure<Guid>(ContentErrors.InvalidSeoData);
        }

        var site = string.IsNullOrWhiteSpace(c.SiteKey) ? ContentConstants.DefaultSiteKey : c.SiteKey.Trim();
        var locale = string.IsNullOrWhiteSpace(c.Locale) ? ContentConstants.DefaultLocale : c.Locale.Trim();
        var slug = string.IsNullOrWhiteSpace(c.Slug)
            ? await slugs.GenerateUniqueAsync(c.Title,
                (candidate, token) => contents.ExistsBySlugAsync(site, locale, candidate, null, token), ct)
            : slugs.Generate(c.Slug);

        if (await contents.ExistsBySlugAsync(site, locale, slug, null, ct))
            return Result.Failure<Guid>(ContentErrors.ContentSlugAlreadyExists);

        var item = ContentItem.Create(type, c.Title, slug, blocksJson, c.AuthorUserId, c.CreatedBy, site, locale);
        item.Update(c.Title, slug, c.Excerpt, blocksJson, c.RenderedHtml, c.FeaturedMediaId, c.SortOrder, c.IsFeatured, locale, c.CreatedBy);
        item.ConfigureCmsMetadata(c.ParentContentId, c.TranslationGroupId, c.TemplateKey, c.UnpublishAtUtc,
            c.SitemapPriority, c.SitemapChangeFrequency, c.CreatedBy);
        item.SetSeo(seo.Title, seo.Description, seo.Keywords, seo.CanonicalUrl, seo.Robots,
            seo.OpenGraphMediaId, seo.StructuredDataJson, c.CreatedBy);
        item.ReplaceTaxonomies(c.TaxonomyTermIds);

        await contents.AddAsync(item, ct);
        await audit.PublishAsync(new ContentAuditEvent(ContentAuditEventTypes.ContentCreated, ContentAuditActions.Created,
            ContentAuditEntityTypes.ContentItem, item.Id, c.CreatedBy, After: ContentAuditSnapshots.From(item),
            Payload: new { item.Id, item.SiteKey, item.Locale, item.Slug, Type = item.Type.ToString() }), ct);
        await unitOfWork.SaveChangesAsync(ct);
        await cache.RemoveContentAsync(item.SiteKey, item.Slug, ct);
        return Result.Success(item.Id);
    }
}
