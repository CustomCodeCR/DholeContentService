using Dhole.Content.Domain.Content;

namespace Dhole.Content.Application.Auditing;

public static class ContentAuditActions
{
    public const string Created = "created";
    public const string Updated = "updated";
    public const string Deleted = "deleted";
    public const string Published = "published";
    public const string Unpublished = "unpublished";
    public const string Scheduled = "scheduled";
    public const string SubmittedForReview = "submitted_for_review";
    public const string Archived = "archived";
    public const string Restored = "restored";
}

public static class ContentAuditEntityTypes
{
    public const string Content = "Content";
    public const string Taxonomy = "TaxonomyTerm";
    public const string Media = "Media";
    public const string Menu = "NavigationMenu";
    public const string Setting = "SiteSetting";
}

public static class ContentAuditEventTypes
{
    public const string ContentCreated = "content.content.created";
    public const string ContentUpdated = "content.content.updated";
    public const string ContentPublished = "content.content.published";
    public const string ContentScheduled = "content.content.scheduled";
    public const string ContentSubmittedForReview = "content.content.submitted-for-review";
    public const string ContentUnpublished = "content.content.unpublished";
    public const string ContentArchived = "content.content.archived";
    public const string ContentDeleted = "content.content.deleted";
    public const string ContentRevisionRestored = "content.content.revision-restored";
    public const string TaxonomyCreated = "content.taxonomy.created";
    public const string MediaRegistered = "content.media.registered";
    public const string MenuReplaced = "content.menu.replaced";
    public const string SettingUpserted = "content.setting.upserted";
}

public sealed record ContentItemAuditSnapshot(
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
    string? SeoTitle,
    string? SeoDescription,
    string? SeoKeywords,
    string? CanonicalUrl,
    string? Robots,
    Guid? OpenGraphMediaId,
    string? StructuredDataJson,
    DateTime? ScheduledAtUtc,
    DateTime? PublishedAtUtc,
    DateTime UpdatedAtUtc)
{
    public static ContentItemAuditSnapshot From(ContentItem item) => new(
        item.Id,
        item.SiteKey,
        item.Type.ToString(),
        item.Status.ToString(),
        item.Title,
        item.Slug,
        item.Excerpt,
        item.BlocksJson,
        item.RenderedHtml,
        item.FeaturedMediaId,
        item.AuthorUserId,
        item.Locale,
        item.SortOrder,
        item.IsFeatured,
        item.SeoTitle,
        item.SeoDescription,
        item.SeoKeywords,
        item.CanonicalUrl,
        item.Robots,
        item.OpenGraphMediaId,
        item.StructuredDataJson,
        item.ScheduledAtUtc,
        item.PublishedAtUtc,
        item.UpdatedAtUtc);
}

public sealed record TaxonomyAuditSnapshot(Guid Id, string SiteKey, string Kind, string Name, string Slug, string? Description, Guid? ParentId, int SortOrder)
{
    public static TaxonomyAuditSnapshot From(TaxonomyTerm term) => new(term.Id, term.SiteKey, term.Kind, term.Name, term.Slug, term.Description, term.ParentId, term.SortOrder);
}

public sealed record MediaAuditSnapshot(Guid Id, Guid StorageFileId, string FileName, string ContentType, string? AltText, string? Caption, string? MetadataJson)
{
    public static MediaAuditSnapshot From(MediaReference media) => new(media.Id, media.StorageFileId, media.FileName, media.ContentType, media.AltText, media.Caption, media.MetadataJson);
}

public sealed record SettingAuditSnapshot(Guid Id, string SiteKey, string Key, string ValueJson, bool IsPublic, DateTime UpdatedAtUtc)
{
    public static SettingAuditSnapshot From(SiteSetting setting) => new(setting.Id, setting.SiteKey, setting.Key, setting.ValueJson, setting.IsPublic, setting.UpdatedAtUtc);
}
