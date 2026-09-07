namespace Dhole.Content.Contracts.Taxonomies;
public sealed record TaxonomyWriteRequest(string Kind,string Name,string? Slug,string? Description,Guid? ParentId,int SortOrder,string? SiteKey);
