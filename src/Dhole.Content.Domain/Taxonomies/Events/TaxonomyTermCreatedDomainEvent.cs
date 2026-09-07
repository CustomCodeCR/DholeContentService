using CustomCodeFramework.Core.Domain.Events;
namespace Dhole.Content.Domain.Taxonomies.Events;
public sealed record TaxonomyTermCreatedDomainEvent(Guid id, string siteKey, string kind, string slug, string name, Guid? actorUserId) : DomainEvent;
