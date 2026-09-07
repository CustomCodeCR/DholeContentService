namespace Dhole.Content.Domain.Content;

public enum ContentType
{
    Page = 0,
    News = 1,
    Post = 2,
    Announcement = 3,
    Banner = 4,
    Video = 5,
    ReusableBlock = 6
}

public enum ContentStatus
{
    Draft = 0,
    PendingReview = 1,
    Scheduled = 2,
    Published = 3,
    Archived = 4
}

public sealed class ContentItem
{
    private ContentItem() { }

    public Guid Id { get; private set; }
    public string SiteKey { get; private set; } = "main";
    public ContentType Type { get; private set; }
    public ContentStatus Status { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string? Excerpt { get; private set; }
    public string BlocksJson { get; private set; } = "[]";
    public string? RenderedHtml { get; private set; }
    public Guid? FeaturedMediaId { get; private set; }
    public Guid? AuthorUserId { get; private set; }
    public string Locale { get; private set; } = "es-CR";
    public int SortOrder { get; private set; }
    public bool IsFeatured { get; private set; }

    public string? SeoTitle { get; private set; }
    public string? SeoDescription { get; private set; }
    public string? SeoKeywords { get; private set; }
    public string? CanonicalUrl { get; private set; }
    public string? Robots { get; private set; }
    public Guid? OpenGraphMediaId { get; private set; }
    public string? StructuredDataJson { get; private set; }

    public DateTime? ScheduledAtUtc { get; private set; }
    public DateTime? PublishedAtUtc { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }
    public DateTime? DeletedAtUtc { get; private set; }
    public Guid? CreatedBy { get; private set; }
    public Guid? UpdatedBy { get; private set; }

    public ICollection<ContentTaxonomy> Taxonomies { get; private set; } = [];
    public ICollection<ContentRevision> Revisions { get; private set; } = [];

    public static ContentItem Create(
        ContentType type,
        string title,
        string slug,
        string blocksJson,
        Guid? authorUserId,
        Guid? actorUserId,
        string? siteKey = null,
        string? locale = null)
    {
        var now = DateTime.UtcNow;
        return new ContentItem
        {
            Id = Guid.NewGuid(),
            Type = type,
            Status = ContentStatus.Draft,
            Title = NormalizeRequired(title, nameof(title)),
            Slug = NormalizeSlug(slug),
            BlocksJson = string.IsNullOrWhiteSpace(blocksJson) ? "[]" : blocksJson,
            AuthorUserId = authorUserId ?? actorUserId,
            SiteKey = string.IsNullOrWhiteSpace(siteKey) ? "main" : siteKey.Trim(),
            Locale = string.IsNullOrWhiteSpace(locale) ? "es-CR" : locale.Trim(),
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
            CreatedBy = actorUserId,
            UpdatedBy = actorUserId
        };
    }

    public void Update(
        string title,
        string slug,
        string? excerpt,
        string blocksJson,
        string? renderedHtml,
        Guid? featuredMediaId,
        int sortOrder,
        bool isFeatured,
        string? locale,
        Guid? actorUserId)
    {
        Title = NormalizeRequired(title, nameof(title));
        Slug = NormalizeSlug(slug);
        Excerpt = NormalizeOptional(excerpt);
        BlocksJson = string.IsNullOrWhiteSpace(blocksJson) ? "[]" : blocksJson;
        RenderedHtml = NormalizeOptional(renderedHtml);
        FeaturedMediaId = featuredMediaId;
        SortOrder = sortOrder;
        IsFeatured = isFeatured;
        Locale = string.IsNullOrWhiteSpace(locale) ? Locale : locale.Trim();
        Touch(actorUserId);
    }

    public void SetSeo(
        string? title,
        string? description,
        string? keywords,
        string? canonicalUrl,
        string? robots,
        Guid? openGraphMediaId,
        string? structuredDataJson,
        Guid? actorUserId)
    {
        SeoTitle = NormalizeOptional(title);
        SeoDescription = NormalizeOptional(description);
        SeoKeywords = NormalizeOptional(keywords);
        CanonicalUrl = NormalizeOptional(canonicalUrl);
        Robots = NormalizeOptional(robots) ?? "index,follow";
        OpenGraphMediaId = openGraphMediaId;
        StructuredDataJson = NormalizeOptional(structuredDataJson);
        Touch(actorUserId);
    }

    public void SubmitForReview(Guid? actorUserId)
    {
        EnsureNotArchived();
        Status = ContentStatus.PendingReview;
        ScheduledAtUtc = null;
        Touch(actorUserId);
    }

    public void Publish(DateTime utcNow, Guid? actorUserId)
    {
        EnsureNotArchived();
        Status = ContentStatus.Published;
        PublishedAtUtc = utcNow;
        ScheduledAtUtc = null;
        Touch(actorUserId);
    }

    public void Schedule(DateTime scheduledAtUtc, Guid? actorUserId)
    {
        EnsureNotArchived();
        if (scheduledAtUtc <= DateTime.UtcNow)
            throw new ArgumentException("La fecha programada debe estar en el futuro.");
        Status = ContentStatus.Scheduled;
        ScheduledAtUtc = scheduledAtUtc;
        Touch(actorUserId);
    }

    public void Unpublish(Guid? actorUserId)
    {
        EnsureNotArchived();
        Status = ContentStatus.Draft;
        ScheduledAtUtc = null;
        Touch(actorUserId);
    }

    public void Archive(Guid? actorUserId)
    {
        Status = ContentStatus.Archived;
        ScheduledAtUtc = null;
        Touch(actorUserId);
    }

    public void SoftDelete(Guid? actorUserId)
    {
        DeletedAtUtc = DateTime.UtcNow;
        Touch(actorUserId);
    }

    public ContentRevision Snapshot(Guid? actorUserId, string? reason = null)
        => ContentRevision.Create(this, actorUserId, reason);

    public void Restore(ContentRevision revision, Guid? actorUserId)
    {
        Title = revision.Title;
        Slug = revision.Slug;
        Excerpt = revision.Excerpt;
        BlocksJson = revision.BlocksJson;
        RenderedHtml = revision.RenderedHtml;
        FeaturedMediaId = revision.FeaturedMediaId;
        SeoTitle = revision.SeoTitle;
        SeoDescription = revision.SeoDescription;
        SeoKeywords = revision.SeoKeywords;
        CanonicalUrl = revision.CanonicalUrl;
        Robots = revision.Robots;
        OpenGraphMediaId = revision.OpenGraphMediaId;
        StructuredDataJson = revision.StructuredDataJson;
        Status = ContentStatus.Draft;
        ScheduledAtUtc = null;
        Touch(actorUserId);
    }

    private void Touch(Guid? actorUserId)
    {
        UpdatedAtUtc = DateTime.UtcNow;
        UpdatedBy = actorUserId;
    }

    private void EnsureNotArchived()
    {
        if (Status == ContentStatus.Archived)
            throw new InvalidOperationException("El contenido archivado debe restaurarse antes de modificarse.");
    }

    private static string NormalizeRequired(string value, string name)
        => string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException($"{name} es requerido.")
            : value.Trim();

    public static string NormalizeSlug(string value)
    {
        var normalized = NormalizeRequired(value, nameof(value)).Trim('/').ToLowerInvariant();
        return string.Join('-', normalized
            .Split([' ', '_'], StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim()));
    }

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public sealed class ContentRevision
{
    private ContentRevision() { }

    public Guid Id { get; private set; }
    public Guid ContentId { get; private set; }
    public int RevisionNumber { get; set; }
    public string Title { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string? Excerpt { get; private set; }
    public string BlocksJson { get; private set; } = "[]";
    public string? RenderedHtml { get; private set; }
    public Guid? FeaturedMediaId { get; private set; }
    public string? SeoTitle { get; private set; }
    public string? SeoDescription { get; private set; }
    public string? SeoKeywords { get; private set; }
    public string? CanonicalUrl { get; private set; }
    public string? Robots { get; private set; }
    public Guid? OpenGraphMediaId { get; private set; }
    public string? StructuredDataJson { get; private set; }
    public string? Reason { get; private set; }
    public Guid? CreatedBy { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    public ContentItem Content { get; private set; } = null!;

    public static ContentRevision Create(ContentItem item, Guid? actorUserId, string? reason)
        => new()
        {
            Id = Guid.NewGuid(),
            ContentId = item.Id,
            Title = item.Title,
            Slug = item.Slug,
            Excerpt = item.Excerpt,
            BlocksJson = item.BlocksJson,
            RenderedHtml = item.RenderedHtml,
            FeaturedMediaId = item.FeaturedMediaId,
            SeoTitle = item.SeoTitle,
            SeoDescription = item.SeoDescription,
            SeoKeywords = item.SeoKeywords,
            CanonicalUrl = item.CanonicalUrl,
            Robots = item.Robots,
            OpenGraphMediaId = item.OpenGraphMediaId,
            StructuredDataJson = item.StructuredDataJson,
            Reason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim(),
            CreatedBy = actorUserId,
            CreatedAtUtc = DateTime.UtcNow
        };
}

public sealed class TaxonomyTerm
{
    private TaxonomyTerm() { }
    public Guid Id { get; private set; }
    public string SiteKey { get; private set; } = "main";
    public string Kind { get; private set; } = "category";
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public Guid? ParentId { get; private set; }
    public int SortOrder { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    public static TaxonomyTerm Create(string kind, string name, string slug, string? description, Guid? parentId, int sortOrder, string? siteKey)
        => new()
        {
            Id = Guid.NewGuid(),
            Kind = string.Equals(kind, "tag", StringComparison.OrdinalIgnoreCase) ? "tag" : "category",
            Name = name.Trim(),
            Slug = ContentItem.NormalizeSlug(slug),
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            ParentId = parentId,
            SortOrder = sortOrder,
            SiteKey = string.IsNullOrWhiteSpace(siteKey) ? "main" : siteKey.Trim(),
            CreatedAtUtc = DateTime.UtcNow
        };
}

public sealed class ContentTaxonomy
{
    public Guid ContentId { get; set; }
    public Guid TaxonomyTermId { get; set; }
    public ContentItem Content { get; set; } = null!;
    public TaxonomyTerm TaxonomyTerm { get; set; } = null!;
}

public sealed class MediaReference
{
    private MediaReference() { }
    public Guid Id { get; private set; }
    public Guid StorageFileId { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public string? AltText { get; private set; }
    public string? Caption { get; private set; }
    public string? MetadataJson { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public Guid? CreatedBy { get; private set; }

    public static MediaReference Create(Guid storageFileId, string fileName, string contentType, string? altText, string? caption, string? metadataJson, Guid? actor)
        => new()
        {
            Id = Guid.NewGuid(),
            StorageFileId = storageFileId,
            FileName = fileName.Trim(),
            ContentType = contentType.Trim(),
            AltText = string.IsNullOrWhiteSpace(altText) ? null : altText.Trim(),
            Caption = string.IsNullOrWhiteSpace(caption) ? null : caption.Trim(),
            MetadataJson = string.IsNullOrWhiteSpace(metadataJson) ? null : metadataJson,
            CreatedAtUtc = DateTime.UtcNow,
            CreatedBy = actor
        };
}

public sealed class NavigationMenu
{
    private NavigationMenu() { }
    public Guid Id { get; private set; }
    public string SiteKey { get; private set; } = "main";
    public string Name { get; private set; } = string.Empty;
    public string Location { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;
    public DateTime UpdatedAtUtc { get; private set; }
    public ICollection<NavigationMenuItem> Items { get; private set; } = [];

    public static NavigationMenu Create(string name, string location, string? siteKey)
        => new()
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Location = location.Trim().ToLowerInvariant(),
            SiteKey = string.IsNullOrWhiteSpace(siteKey) ? "main" : siteKey.Trim(),
            UpdatedAtUtc = DateTime.UtcNow
        };
}

public sealed class NavigationMenuItem
{
    private NavigationMenuItem() { }
    public Guid Id { get; private set; }
    public Guid MenuId { get; private set; }
    public Guid? ParentId { get; private set; }
    public string Label { get; private set; } = string.Empty;
    public string? Url { get; private set; }
    public Guid? ContentId { get; private set; }
    public string Target { get; private set; } = "_self";
    public int SortOrder { get; private set; }
    public bool IsVisible { get; private set; } = true;
    public NavigationMenu Menu { get; private set; } = null!;

    public static NavigationMenuItem Create(Guid menuId, string label, string? url, Guid? contentId, Guid? parentId, int sortOrder, string? target)
        => new()
        {
            Id = Guid.NewGuid(),
            MenuId = menuId,
            Label = label.Trim(),
            Url = string.IsNullOrWhiteSpace(url) ? null : url.Trim(),
            ContentId = contentId,
            ParentId = parentId,
            SortOrder = sortOrder,
            Target = string.IsNullOrWhiteSpace(target) ? "_self" : target.Trim()
        };
}

public sealed class SiteSetting
{
    private SiteSetting() { }
    public Guid Id { get; private set; }
    public string SiteKey { get; private set; } = "main";
    public string Key { get; private set; } = string.Empty;
    public string ValueJson { get; private set; } = "null";
    public bool IsPublic { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }
    public Guid? UpdatedBy { get; private set; }

    public static SiteSetting Create(string key, string valueJson, bool isPublic, string? siteKey, Guid? actor)
        => new()
        {
            Id = Guid.NewGuid(),
            Key = key.Trim(),
            ValueJson = valueJson,
            IsPublic = isPublic,
            SiteKey = string.IsNullOrWhiteSpace(siteKey) ? "main" : siteKey.Trim(),
            UpdatedAtUtc = DateTime.UtcNow,
            UpdatedBy = actor
        };

    public void Update(string valueJson, bool isPublic, Guid? actor)
    {
        ValueJson = valueJson;
        IsPublic = isPublic;
        UpdatedAtUtc = DateTime.UtcNow;
        UpdatedBy = actor;
    }
}
