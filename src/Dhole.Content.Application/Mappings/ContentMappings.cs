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
    public static ContentItemDto ToDto(ContentItem x)=>new(x.Id,x.SiteKey,x.Type.ToString(),x.Status.ToString(),x.Title,x.Slug,x.Excerpt,x.BlocksJson,x.RenderedHtml,x.FeaturedMediaId,x.AuthorUserId,x.Locale,x.SortOrder,x.IsFeatured,new SeoDto(x.SeoTitle,x.SeoDescription,x.SeoKeywords,x.CanonicalUrl,x.Robots,x.OpenGraphMediaId,x.StructuredDataJson),x.ScheduledAtUtc,x.PublishedAtUtc,x.CreatedAtUtc,x.UpdatedAtUtc);
    public static TaxonomyTermDto ToDto(TaxonomyTerm x)=>new(x.Id,x.SiteKey,x.Kind,x.Name,x.Slug,x.Description,x.ParentId,x.SortOrder,x.CreatedAtUtc,x.UpdatedAtUtc);
    public static MediaDto ToDto(MediaReference x)=>new(x.Id,x.StorageFileId,x.FileName,x.ContentType,x.AltText,x.Caption,x.MetadataJson,x.CreatedAtUtc,x.UpdatedAtUtc);
    public static NavigationMenuDto ToDto(NavigationMenu x)=>new(x.Id,x.SiteKey,x.Name,x.Location,x.IsActive,x.Items.OrderBy(i=>i.SortOrder).Select(i=>new NavigationMenuItemDto(i.Id,i.ParentId,i.Label,i.Url,i.ContentId,i.Target,i.SortOrder,i.IsVisible)).ToArray(),x.CreatedAtUtc,x.UpdatedAtUtc);
    public static SiteSettingDto ToDto(SiteSetting x)=>new(x.Id,x.SiteKey,x.Key,x.ValueJson,x.IsPublic,x.CreatedAtUtc,x.UpdatedAtUtc);
}
