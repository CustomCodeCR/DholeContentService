namespace Dhole.Content.Contracts.ContentItems;

public sealed record ContentItemDto(
    Guid Id,
    string SiteKey,
    string Type,
    string Status,
    string Title,
    string Slug,
    string? Excerpt,
    string BlocksJson,
    string? RenderedHtml,
    Guid? FeaturedMediaId,
    Guid? AuthorUserId,
    string Locale,
    int SortOrder,
    bool IsFeatured,
    IReadOnlyCollection<Guid> TaxonomyTermIds,
    SeoDto Seo,
    DateTime? ScheduledAtUtc,
    DateTime? PublishedAtUtc,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc
);
