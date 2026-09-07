using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;
namespace Dhole.Content.Application.ContentItems.UpdateContentItem;
public sealed record UpdateContentItemCommand(Guid Id,string Title,string? Slug,string? Excerpt,string BlocksJson,string? RenderedHtml,Guid? FeaturedMediaId,string? Locale,int SortOrder,bool IsFeatured,IReadOnlyCollection<Guid> TaxonomyTermIds,string? SeoTitle,string? SeoDescription,string? SeoKeywords,string? CanonicalUrl,string? Robots,Guid? OpenGraphMediaId,string? StructuredDataJson,Guid? UpdatedBy):ICommand<Result>;
