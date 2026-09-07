using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;
namespace Dhole.Content.Application.ContentItems.CreateContentItem;
public sealed record CreateContentItemCommand(string Type,string Title,string? Slug,string? Excerpt,string BlocksJson,string? RenderedHtml,Guid? FeaturedMediaId,Guid? AuthorUserId,string? Locale,int SortOrder,bool IsFeatured,IReadOnlyCollection<Guid> TaxonomyTermIds,string? SeoTitle,string? SeoDescription,string? SeoKeywords,string? CanonicalUrl,string? Robots,Guid? OpenGraphMediaId,string? StructuredDataJson,string? SiteKey,Guid? CreatedBy):ICommand<Result<Guid>>;
