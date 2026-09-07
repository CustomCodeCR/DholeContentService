namespace Dhole.Content.Contracts.Taxonomies;
public sealed record TaxonomyTermDto(Guid Id,string SiteKey,string Kind,string Name,string Slug,string? Description,Guid? ParentId,int SortOrder,DateTime CreatedAtUtc,DateTime? UpdatedAtUtc);
