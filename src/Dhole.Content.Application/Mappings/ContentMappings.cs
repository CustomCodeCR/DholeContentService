using Dhole.Content.Contracts.ContentItems;
using Dhole.Content.Contracts.Media;
using Dhole.Content.Contracts.Navigation;
using Dhole.Content.Contracts.Settings;
using Dhole.Content.Contracts.Taxonomies;
using Dhole.Content.Domain.ContentItems.Entities;
using Dhole.Content.Domain.Media.Entities;
using Dhole.Content.Domain.Navigation.Entities;
using Dhole.Content.Domain.Settings.Entities;
using Dhole.Content.Domain.Taxonomies.Entities;

namespace Dhole.Content.Application.Mappings;

public static class ContentMappings
{
    public static ContentItemDto ToDto(ContentItem item) =>
        new(
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
            item.ParentContentId,
            item.TranslationGroupId,
            item.TemplateKey,
            item.UnpublishAtUtc,
            item.SitemapPriority,
            item.SitemapChangeFrequency,
            item.SortOrder,
            item.IsFeatured,
            item.Taxonomies.Select(x => x.TaxonomyTermId).ToArray(),
            new SeoDto(
                item.SeoTitle,
                item.SeoDescription,
                item.SeoKeywords,
                item.CanonicalUrl,
                item.Robots,
                item.OpenGraphMediaId,
                item.StructuredDataJson
            ),
            item.ScheduledAtUtc,
            item.PublishedAtUtc,
            item.CreatedAtUtc,
            item.UpdatedAtUtc
        );

    public static TaxonomyTermDto ToDto(TaxonomyTerm item) =>
        new(
            item.Id,
            item.SiteKey,
            item.Kind,
            item.Name,
            item.Slug,
            item.Description,
            item.ParentId,
            item.SortOrder,
            item.CreatedAtUtc,
            item.UpdatedAtUtc
        );

    public static MediaDto ToDto(MediaReference item) =>
        new(
            item.Id,
            item.StorageFileId,
            item.FileName,
            item.ContentType,
            item.AltText,
            item.Caption,
            item.MetadataJson,
            item.CreatedAtUtc,
            item.UpdatedAtUtc
        );

    public static NavigationMenuDto ToDto(NavigationMenu item) =>
        new(
            item.Id,
            item.SiteKey,
            item.Name,
            item.Location,
            item.IsActive,
            item.Items
                .OrderBy(x => x.SortOrder)
                .Select(x => new NavigationMenuItemDto(
                    x.Id,
                    x.ParentId,
                    x.Label,
                    x.Url,
                    x.ContentId,
                    x.Target,
                    x.SortOrder,
                    x.IsVisible
                ))
                .ToArray(),
            item.CreatedAtUtc,
            item.UpdatedAtUtc
        );

    public static SiteSettingDto ToDto(SiteSetting item) =>
        new(
            item.Id,
            item.SiteKey,
            item.Key,
            item.ValueJson,
            item.IsPublic,
            item.CreatedAtUtc,
            item.UpdatedAtUtc
        );
}
