using Dhole.Content.Domain.Taxonomies.Entities;

namespace Dhole.Content.Domain.ContentItems.Entities;

public sealed class ContentTaxonomy
{
    private ContentTaxonomy() { }

    public Guid ContentId { get; private set; }
    public Guid TaxonomyTermId { get; private set; }
    public ContentItem Content { get; private set; } = default!;
    public TaxonomyTerm TaxonomyTerm { get; private set; } = default!;

    internal static ContentTaxonomy Create(Guid contentId, Guid taxonomyTermId)
        => new() { ContentId = contentId, TaxonomyTermId = taxonomyTermId };
}
