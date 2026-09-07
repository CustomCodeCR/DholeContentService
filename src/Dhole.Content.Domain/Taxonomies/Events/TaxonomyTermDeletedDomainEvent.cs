using CustomCodeFramework.Core.Domain.Events;
namespace Dhole.Content.Domain.Taxonomies.Events;
public sealed record TaxonomyTermDeletedDomainEvent(Guid id, string siteKey, string kind, string slug, Guid? actorUserId) : DomainEvent;
