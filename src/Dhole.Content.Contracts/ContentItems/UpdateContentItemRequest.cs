namespace Dhole.Content.Contracts.ContentItems;
public sealed record UpdateContentItemRequest(string Title,string? Slug,string? Excerpt,string? BlocksJson,string? RenderedHtml,Guid? FeaturedMediaId,string? Locale,int SortOrder,bool IsFeatured,IReadOnlyCollection<Guid>? TaxonomyTermIds,SeoWriteRequest? Seo);
