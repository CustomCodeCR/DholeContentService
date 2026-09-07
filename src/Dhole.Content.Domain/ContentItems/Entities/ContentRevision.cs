namespace Dhole.Content.Domain.ContentItems.Entities;

public sealed class ContentRevision
{
    private ContentRevision() { }

    public Guid Id { get; private set; }
    public Guid ContentId { get; private set; }
    public int RevisionNumber { get; private set; }
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
    public ContentItem Content { get; private set; } = default!;

    internal static ContentRevision Create(ContentItem item, int revisionNumber, Guid? actorUserId, string? reason)
        => new()
        {
            Id = Guid.NewGuid(),
            ContentId = item.Id,
            RevisionNumber = revisionNumber,
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
