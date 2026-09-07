using CustomCodeFramework.Core.Domain.Entities;
using Dhole.Content.Domain.ContentItems.Enums;
using Dhole.Content.Domain.ContentItems.Events;

namespace Dhole.Content.Domain.ContentItems.Entities;

public sealed class ContentItem : SoftDeletableAggregateRoot<Guid>
{
    private readonly List<ContentRevision> _revisions = [];
    private readonly List<ContentTaxonomy> _taxonomies = [];

    private ContentItem() { }

    private ContentItem(Guid id, ContentType type, string title, string slug, string blocksJson, Guid? authorUserId, Guid? actorUserId, string siteKey, string locale)
        : base(id)
    {
        Type = type;
        Status = ContentStatus.Draft;
        Title = title.Trim();
        Slug = slug.Trim().ToLowerInvariant();
        BlocksJson = string.IsNullOrWhiteSpace(blocksJson) ? "[]" : blocksJson;
        AuthorUserId = authorUserId ?? actorUserId;
        SiteKey = siteKey.Trim();
        Locale = locale.Trim();
        Robots = "index,follow";
        MarkAsCreated(DateTime.UtcNow, actorUserId?.ToString());
    }

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
    public IReadOnlyCollection<ContentRevision> Revisions => _revisions;
    public IReadOnlyCollection<ContentTaxonomy> Taxonomies => _taxonomies;

    public static ContentItem Create(ContentType type, string title, string slug, string blocksJson, Guid? authorUserId, Guid? actorUserId, string? siteKey = null, string? locale = null)
    {
        var item = new ContentItem(Guid.NewGuid(), type, title, slug, blocksJson, authorUserId, actorUserId,
            string.IsNullOrWhiteSpace(siteKey) ? "main" : siteKey,
            string.IsNullOrWhiteSpace(locale) ? "es-CR" : locale);
        item.AddDomainEvent(new ContentItemCreatedDomainEvent(item.Id, item.SiteKey, item.Type, item.Slug, item.Title, actorUserId));
        return item;
    }

    public void Update(string title, string slug, string? excerpt, string blocksJson, string? renderedHtml, Guid? featuredMediaId, int sortOrder, bool isFeatured, string? locale, Guid? actorUserId)
    {
        EnsureNotArchived();
        Title = title.Trim(); Slug = slug.Trim().ToLowerInvariant(); Excerpt = Normalize(excerpt);
        BlocksJson = string.IsNullOrWhiteSpace(blocksJson) ? "[]" : blocksJson; RenderedHtml = Normalize(renderedHtml);
        FeaturedMediaId = featuredMediaId; SortOrder = sortOrder; IsFeatured = isFeatured;
        if (!string.IsNullOrWhiteSpace(locale)) Locale = locale.Trim();
        MarkAsUpdated(DateTime.UtcNow, actorUserId?.ToString());
        AddDomainEvent(new ContentItemUpdatedDomainEvent(Id, SiteKey, Type, Slug, Title, actorUserId));
    }

    public void SetSeo(string? title, string? description, string? keywords, string? canonicalUrl, string? robots, Guid? openGraphMediaId, string? structuredDataJson, Guid? actorUserId)
    {
        SeoTitle = Normalize(title); SeoDescription = Normalize(description); SeoKeywords = Normalize(keywords);
        CanonicalUrl = Normalize(canonicalUrl); Robots = Normalize(robots) ?? "index,follow";
        OpenGraphMediaId = openGraphMediaId; StructuredDataJson = Normalize(structuredDataJson);
        MarkAsUpdated(DateTime.UtcNow, actorUserId?.ToString());
        AddDomainEvent(new ContentItemUpdatedDomainEvent(Id, SiteKey, Type, Slug, Title, actorUserId));
    }

    public ContentRevision CreateRevision(Guid? actorUserId, string? reason = null)
    {
        var revision = ContentRevision.Create(this, _revisions.Count == 0 ? 1 : _revisions.Max(x => x.RevisionNumber) + 1, actorUserId, reason);
        _revisions.Add(revision);
        return revision;
    }

    public void ReplaceTaxonomies(IEnumerable<Guid> taxonomyTermIds)
    {
        _taxonomies.Clear();
        foreach (var id in taxonomyTermIds.Distinct()) _taxonomies.Add(ContentTaxonomy.Create(Id, id));
    }

    public void SubmitForReview(Guid? actorUserId)
    {
        EnsureNotArchived(); Status = ContentStatus.PendingReview; ScheduledAtUtc = null;
        MarkAsUpdated(DateTime.UtcNow, actorUserId?.ToString());
        AddDomainEvent(new ContentItemSubmittedForReviewDomainEvent(Id, SiteKey, Slug, actorUserId));
    }

    public void Publish(DateTime utcNow, Guid? actorUserId)
    {
        EnsureNotArchived(); Status = ContentStatus.Published; PublishedAtUtc = utcNow; ScheduledAtUtc = null;
        MarkAsUpdated(utcNow, actorUserId?.ToString());
        AddDomainEvent(new ContentItemPublishedDomainEvent(Id, SiteKey, Slug, utcNow, actorUserId));
    }

    public void Schedule(DateTime scheduledAtUtc, DateTime utcNow, Guid? actorUserId)
    {
        EnsureNotArchived(); if (scheduledAtUtc <= utcNow) throw new ArgumentException("La fecha programada debe estar en el futuro.");
        Status = ContentStatus.Scheduled; ScheduledAtUtc = scheduledAtUtc;
        MarkAsUpdated(utcNow, actorUserId?.ToString());
        AddDomainEvent(new ContentItemScheduledDomainEvent(Id, SiteKey, Slug, scheduledAtUtc, actorUserId));
    }

    public void Unpublish(Guid? actorUserId)
    {
        EnsureNotArchived(); Status = ContentStatus.Draft; ScheduledAtUtc = null;
        MarkAsUpdated(DateTime.UtcNow, actorUserId?.ToString());
        AddDomainEvent(new ContentItemUnpublishedDomainEvent(Id, SiteKey, Slug, actorUserId));
    }

    public void Archive(Guid? actorUserId)
    {
        Status = ContentStatus.Archived; ScheduledAtUtc = null;
        MarkAsUpdated(DateTime.UtcNow, actorUserId?.ToString());
        AddDomainEvent(new ContentItemArchivedDomainEvent(Id, SiteKey, Slug, actorUserId));
    }

    public void RestoreRevision(ContentRevision revision, Guid? actorUserId)
    {
        Title = revision.Title; Slug = revision.Slug; Excerpt = revision.Excerpt; BlocksJson = revision.BlocksJson;
        RenderedHtml = revision.RenderedHtml; FeaturedMediaId = revision.FeaturedMediaId; SeoTitle = revision.SeoTitle;
        SeoDescription = revision.SeoDescription; SeoKeywords = revision.SeoKeywords; CanonicalUrl = revision.CanonicalUrl;
        Robots = revision.Robots; OpenGraphMediaId = revision.OpenGraphMediaId; StructuredDataJson = revision.StructuredDataJson;
        Status = ContentStatus.Draft; ScheduledAtUtc = null;
        MarkAsUpdated(DateTime.UtcNow, actorUserId?.ToString());
        AddDomainEvent(new ContentItemRevisionRestoredDomainEvent(Id, revision.Id, SiteKey, Slug, actorUserId));
    }

    public void Delete(Guid? actorUserId)
    {
        MarkAsDeleted(DateTime.UtcNow, actorUserId?.ToString());
        AddDomainEvent(new ContentItemDeletedDomainEvent(Id, SiteKey, Slug, actorUserId));
    }

    private void EnsureNotArchived() { if (Status == ContentStatus.Archived) throw new InvalidOperationException("El contenido archivado no puede modificarse."); }
    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
