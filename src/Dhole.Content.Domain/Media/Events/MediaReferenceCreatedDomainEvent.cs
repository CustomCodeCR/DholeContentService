using CustomCodeFramework.Core.Domain.Events;
namespace Dhole.Content.Domain.Media.Events;
public sealed record MediaReferenceCreatedDomainEvent(Guid id, Guid storageFileId, string fileName, Guid? actorUserId) : DomainEvent;
