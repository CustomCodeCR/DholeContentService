namespace Dhole.Content.Contracts;

public sealed record ContentWriteRequest(
    string Type,
    string Title,
    string Slug,
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
    string? SiteKey);

public sealed record SeoWriteRequest(
    string? Title,
    string? Description,
    string? Keywords,
    string? CanonicalUrl,
    string? Robots,
    Guid? OpenGraphMediaId,
    string? StructuredDataJson);

public sealed record ScheduleContentRequest(DateTime ScheduledAtUtc);
public sealed record RevisionRestoreRequest(string? Reason);
public sealed record TaxonomyWriteRequest(string Kind, string Name, string Slug, string? Description, Guid? ParentId, int SortOrder, string? SiteKey);
public sealed record MenuWriteRequest(string Name, string Location, string? SiteKey, IReadOnlyCollection<MenuItemWriteRequest> Items);
public sealed record MenuItemWriteRequest(string Label, string? Url, Guid? ContentId, Guid? ParentId, int SortOrder, string? Target);
public sealed record SiteSettingWriteRequest(string Key, string ValueJson, bool IsPublic, string? SiteKey);
public sealed record MediaRegisterRequest(Guid StorageFileId, string FileName, string ContentType, string? AltText, string? Caption, string? MetadataJson);

public sealed record ContentResponse(
    Guid Id,
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
    SeoResponse Seo,
    DateTime? ScheduledAtUtc,
    DateTime? PublishedAtUtc,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

public sealed record SeoResponse(
    string? Title,
    string? Description,
    string? Keywords,
    string? CanonicalUrl,
    string? Robots,
    Guid? OpenGraphMediaId,
    string? StructuredDataJson);

public sealed record RevisionResponse(Guid Id, int RevisionNumber, string Title, string Slug, string? Reason, Guid? CreatedBy, DateTime CreatedAtUtc);
public sealed record PublicMenuResponse(Guid Id, string Name, string Location, IReadOnlyCollection<PublicMenuItemResponse> Items);
public sealed record PublicMenuItemResponse(Guid Id, Guid? ParentId, string Label, string? Url, Guid? ContentId, string Target, int SortOrder);
public sealed record MediaResponse(Guid Id, Guid StorageFileId, string FileName, string ContentType, string? AltText, string? Caption, string? MetadataJson, DateTime CreatedAtUtc);
public sealed record PagedResult<T>(IReadOnlyCollection<T> Items, int Page, int PageSize, long Total);
