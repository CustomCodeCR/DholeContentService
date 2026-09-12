namespace Dhole.Content.Contracts.Editor;

public sealed record EditorSeoRequest(
    string? Title,
    string? Description,
    string? Keywords,
    string? CanonicalUrl,
    string? Robots,
    Guid? OpenGraphMediaId,
    string? StructuredDataJson
);

public sealed record EditorContentRequest(
    string Type,
    string Title,
    string? ContentHtml,
    string? Slug,
    string? Excerpt,
    Guid? FeaturedMediaId,
    string? Locale,
    int? SortOrder,
    bool? IsFeatured,
    IReadOnlyCollection<Guid>? CategoryIds,
    EditorSeoRequest? Seo,
    string? SiteKey,
    Guid? ParentContentId,
    Guid? TranslationGroupId,
    string? TemplateKey,
    DateTime? UnpublishAtUtc,
    decimal? SitemapPriority,
    string? SitemapChangeFrequency
);

public sealed record EditorDashboardDto(
    long Pages,
    long News,
    long Banners,
    long Media,
    long Drafts,
    long PendingReview,
    long Scheduled,
    long Published
);

public sealed record EditorOptionDto(string Value, string Label);

public sealed record EditorOptionsDto(
    IReadOnlyCollection<EditorOptionDto> ContentTypes,
    IReadOnlyCollection<EditorOptionDto> Statuses,
    string DefaultLocale,
    string DefaultSiteKey
);
