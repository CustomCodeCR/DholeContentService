using CustomCodeFramework.Core.Domain.Entities;
using Dhole.Content.Domain.Taxonomies.Events;
namespace Dhole.Content.Domain.Taxonomies.Entities;

public sealed class TaxonomyTerm : SoftDeletableAggregateRoot<Guid>
{
    private TaxonomyTerm() { }
    private TaxonomyTerm(Guid id, string kind, string name, string slug, string? description, Guid? parentId, int sortOrder, string siteKey, Guid? actor) : base(id)
    {
        Kind = string.Equals(kind, "tag", StringComparison.OrdinalIgnoreCase) ? "tag" : "category";
        Name = name.Trim(); Slug = slug.Trim().ToLowerInvariant(); Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        ParentId = parentId; SortOrder = sortOrder; SiteKey = siteKey.Trim(); MarkAsCreated(DateTime.UtcNow, actor?.ToString());
    }
    public string SiteKey { get; private set; } = "main";
    public string Kind { get; private set; } = "category";
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public Guid? ParentId { get; private set; }
    public int SortOrder { get; private set; }
    public static TaxonomyTerm Create(string kind,string name,string slug,string? description,Guid? parentId,int sortOrder,string? siteKey,Guid? actor)
    {
        var term = new TaxonomyTerm(Guid.NewGuid(),kind,name,slug,description,parentId,sortOrder,string.IsNullOrWhiteSpace(siteKey)?"main":siteKey,actor);
        term.AddDomainEvent(new TaxonomyTermCreatedDomainEvent(term.Id,term.SiteKey,term.Kind,term.Slug,term.Name,actor)); return term;
    }
    public void Update(string name,string slug,string? description,Guid? parentId,int sortOrder,Guid? actor)
    { Name=name.Trim(); Slug=slug.Trim().ToLowerInvariant(); Description=string.IsNullOrWhiteSpace(description)?null:description.Trim(); ParentId=parentId; SortOrder=sortOrder; MarkAsUpdated(DateTime.UtcNow,actor?.ToString()); AddDomainEvent(new TaxonomyTermUpdatedDomainEvent(Id,SiteKey,Kind,Slug,Name,actor)); }
    public void Delete(Guid? actor) { MarkAsDeleted(DateTime.UtcNow,actor?.ToString()); AddDomainEvent(new TaxonomyTermDeletedDomainEvent(Id,SiteKey,Kind,Slug,actor)); }
}
