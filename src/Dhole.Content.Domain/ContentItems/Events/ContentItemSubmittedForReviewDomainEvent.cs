using CustomCodeFramework.Core.Domain.Events;
namespace Dhole.Content.Domain.ContentItems.Events;
public sealed record ContentItemSubmittedForReviewDomainEvent(Guid id, string siteKey, string slug, Guid? actorUserId) : DomainEvent;
