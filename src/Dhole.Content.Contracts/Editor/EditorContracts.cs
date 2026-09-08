namespace Dhole.Content.Contracts.Editor;

public sealed record EditorSeoRequest(
    string? Title,
    string? Description,
    string? Keywords,
    string? CanonicalUrl,
    Guid? OpenGraphMediaId
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
    string? SiteKey
);

public sealed record EditorDashboardDto(
    int Pages,
    int News,
    int Banners,
    int Media,
    int Drafts,
    int PendingReview,
    int Scheduled,
    int Published
);

public sealed record EditorOptionDto(string Value, string Label);

public sealed record EditorOptionsDto(
    IReadOnlyCollection<EditorOptionDto> ContentTypes,
    IReadOnlyCollection<EditorOptionDto> Statuses,
    string DefaultLocale,
    string DefaultSiteKey
);
