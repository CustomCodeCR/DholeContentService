namespace Dhole.Content.Contracts.ContentItems;
public sealed record ContentItemListDto(Guid Id,string SiteKey,string Type,string Status,string Title,string Slug,string? Excerpt,Guid? FeaturedMediaId,string Locale,bool IsFeatured,DateTime? PublishedAtUtc,DateTime CreatedAtUtc,DateTime? UpdatedAtUtc);
