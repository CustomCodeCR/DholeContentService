using CustomCodeFramework.Core.Domain.Events;
using Dhole.Content.Domain.ContentItems.Enums;

namespace Dhole.Content.Domain.ContentItems.Events;

public sealed record ContentItemUpdatedDomainEvent(Guid id, string siteKey, ContentType type, string slug, string title, Guid? actorUserId) : DomainEvent;
