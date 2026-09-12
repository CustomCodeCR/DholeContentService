using Dhole.Content.Domain.ContentItems.Entities;
using Dhole.Content.Domain.Media.Entities;
using Dhole.Content.Domain.Navigation.Entities;
using Dhole.Content.Domain.Settings.Entities;
using Dhole.Content.Domain.Taxonomies.Entities;

namespace Dhole.Content.Application.Auditing;

public static class ContentAuditSnapshots
{
    public static object From(ContentItem x) => new
    {
        x.Id,
        x.SiteKey,
        Type = x.Type.ToString(),
        Status = x.Status.ToString(),
        x.Title,
        x.Slug,
        x.Excerpt,
        x.BlocksJson,
        x.RenderedHtml,
        x.FeaturedMediaId,
        x.AuthorUserId,
        x.Locale,
        x.ParentContentId,
        x.TranslationGroupId,
        x.TemplateKey,
        x.UnpublishAtUtc,
        x.SitemapPriority,
        x.SitemapChangeFrequency,
        x.SortOrder,
        x.IsFeatured,
        x.SeoTitle,
        x.SeoDescription,
        x.SeoKeywords,
        x.CanonicalUrl,
        x.Robots,
        x.OpenGraphMediaId,
        x.StructuredDataJson,
        x.ScheduledAtUtc,
        x.PublishedAtUtc,
        x.IsDeleted,
        x.CreatedAtUtc,
        x.UpdatedAtUtc
    };

    public static object From(TaxonomyTerm x) => new { x.Id, x.SiteKey, x.Kind, x.Name, x.Slug, x.Description, x.ParentId, x.SortOrder, x.IsDeleted, x.CreatedAtUtc, x.UpdatedAtUtc };
    public static object From(MediaReference x) => new { x.Id, x.StorageFileId, x.FileName, x.ContentType, x.AltText, x.Caption, x.MetadataJson, x.IsDeleted, x.CreatedAtUtc, x.UpdatedAtUtc };
    public static object From(NavigationMenu x) => new { x.Id, x.SiteKey, x.Name, x.Location, x.IsActive, x.IsDeleted, Items = x.Items.Select(i => new { i.Id, i.ParentId, i.Label, i.Url, i.ContentId, i.Target, i.SortOrder, i.IsVisible }), x.CreatedAtUtc, x.UpdatedAtUtc };
    public static object From(SiteSetting x) => new { x.Id, x.SiteKey, x.Key, x.ValueJson, x.IsPublic, x.IsDeleted, x.CreatedAtUtc, x.UpdatedAtUtc };
}
