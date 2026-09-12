namespace Dhole.Content.Contracts.ContentItems;

public sealed record CreateContentItemRequest(
    string Type,
    string Title,
    string? Slug,
    string? Excerpt,
    string? BlocksJson,
    string? RenderedHtml,
    Guid? FeaturedMediaId,
    Guid? AuthorUserId,
    string? Locale,
    int SortOrder,
    bool IsFeatured,
    IReadOnlyCollection<Guid>? TaxonomyTermIds,
    SeoWriteRequest? Seo,
    string? SiteKey,
    Guid? ParentContentId,
    Guid? TranslationGroupId,
    string? TemplateKey,
    DateTime? UnpublishAtUtc,
    decimal? SitemapPriority,
    string? SitemapChangeFrequency
);
