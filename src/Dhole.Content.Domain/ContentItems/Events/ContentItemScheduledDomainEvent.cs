using CustomCodeFramework.Core.Domain.Events;
namespace Dhole.Content.Domain.ContentItems.Events;
public sealed record ContentItemScheduledDomainEvent(Guid id, string siteKey, string slug, DateTime scheduledAtUtc, Guid? actorUserId) : DomainEvent;
