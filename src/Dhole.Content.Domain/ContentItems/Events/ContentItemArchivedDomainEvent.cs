using CustomCodeFramework.Core.Domain.Events;
namespace Dhole.Content.Domain.ContentItems.Events;
public sealed record ContentItemArchivedDomainEvent(Guid id, string siteKey, string slug, Guid? actorUserId) : DomainEvent;
