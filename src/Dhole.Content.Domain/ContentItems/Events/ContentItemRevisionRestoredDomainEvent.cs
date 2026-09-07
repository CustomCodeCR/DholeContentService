using CustomCodeFramework.Core.Domain.Events;
namespace Dhole.Content.Domain.ContentItems.Events;
public sealed record ContentItemRevisionRestoredDomainEvent(Guid id, Guid revisionId, string siteKey, string slug, Guid? actorUserId) : DomainEvent;
