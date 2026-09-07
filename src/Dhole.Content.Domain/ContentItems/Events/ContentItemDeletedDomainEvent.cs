using CustomCodeFramework.Core.Domain.Events;
namespace Dhole.Content.Domain.ContentItems.Events;
public sealed record ContentItemDeletedDomainEvent(Guid id, string siteKey, string slug, Guid? actorUserId) : DomainEvent;
