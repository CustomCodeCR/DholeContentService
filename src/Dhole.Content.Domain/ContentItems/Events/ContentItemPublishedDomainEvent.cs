using CustomCodeFramework.Core.Domain.Events;
namespace Dhole.Content.Domain.ContentItems.Events;
public sealed record ContentItemPublishedDomainEvent(Guid id, string siteKey, string slug, DateTime publishedAtUtc, Guid? actorUserId) : DomainEvent;
