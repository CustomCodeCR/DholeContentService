using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Content.Application.Abstractions.Auditing;
using Dhole.Content.Application.Abstractions.Cache;
using Dhole.Content.Application.Abstractions.Repositories;
using Dhole.Content.Application.Auditing;
using Dhole.Content.Domain.Collections.Entities;
using Dhole.Content.Domain.Shared;

namespace Dhole.Content.Application.Collections;

public sealed record CreateCollectionCommand(string SiteKey, string Code, string Name, string? SettingsJson, bool IsActive, Guid? ActorUserId) : ICommand<Result<Guid>>;
public sealed record UpdateCollectionCommand(Guid Id, string SiteKey, string Code, string Name, string? SettingsJson, bool IsActive, Guid? ActorUserId) : ICommand<Result>;
public sealed record DeleteCollectionCommand(Guid Id, Guid? ActorUserId) : ICommand<Result>;

public sealed class CreateCollectionCommandHandler(ISiteRepository sites, ICollectionRepository collections, IContentAuditService audit, IContentCacheService cache, IUnitOfWork unitOfWork) : ICommandHandler<CreateCollectionCommand, Result<Guid>>
{
    public async Task<Result<Guid>> HandleAsync(CreateCollectionCommand command, CancellationToken ct = default)
    {
        var site = await sites.GetBySiteKeyAsync(command.SiteKey, ct); if (site is null || site.IsDeleted) return Result.Failure<Guid>(ContentErrors.SiteNotFound); if (await collections.ExistsByCodeAsync(command.SiteKey, command.Code, null, ct)) return Result.Failure<Guid>(ContentErrors.CollectionCodeAlreadyExists);
        ContentCollection collection; try { collection = ContentCollection.Create(command.SiteKey, command.Code, command.Name, command.SettingsJson, command.IsActive, command.ActorUserId); } catch (ArgumentException) { return Result.Failure<Guid>(ContentErrors.InvalidCollectionData); }
        await collections.AddAsync(collection, ct); await audit.PublishAsync(new ContentAuditEvent(ContentAuditEventTypes.CollectionCreated, ContentAuditActions.Created, ContentAuditEntityTypes.Collection, collection.Id, command.ActorUserId, After: Snapshot(collection)), ct); await unitOfWork.SaveChangesAsync(ct); await cache.InvalidateSiteAsync(collection.SiteKey, ct); return Result.Success(collection.Id);
    }
    internal static object Snapshot(ContentCollection collection) => new { collection.Id, collection.SiteKey, collection.Code, collection.Name, collection.SettingsJson, collection.IsActive };
}

public sealed class UpdateCollectionCommandHandler(ISiteRepository sites, ICollectionRepository collections, IContentAuditService audit, IContentCacheService cache, IUnitOfWork unitOfWork) : ICommandHandler<UpdateCollectionCommand, Result>
{
    public async Task<Result> HandleAsync(UpdateCollectionCommand command, CancellationToken ct = default)
    {
        var collection = await collections.GetByIdAsync(command.Id, ct); if (collection is null || collection.IsDeleted) return Result.Failure(ContentErrors.CollectionNotFound); var previousSiteKey = collection.SiteKey;
        var site = await sites.GetBySiteKeyAsync(command.SiteKey, ct); if (site is null || site.IsDeleted) return Result.Failure(ContentErrors.SiteNotFound); if (await collections.ExistsByCodeAsync(command.SiteKey, command.Code, command.Id, ct)) return Result.Failure(ContentErrors.CollectionCodeAlreadyExists);
        var before = CreateCollectionCommandHandler.Snapshot(collection); var previousIsActive = collection.IsActive; try { collection.Update(command.SiteKey, command.Code, command.Name, command.SettingsJson, command.IsActive, command.ActorUserId); } catch (ArgumentException) { return Result.Failure(ContentErrors.InvalidCollectionData); }
        await audit.PublishAsync(new ContentAuditEvent(ContentAuditEventTypes.CollectionUpdated, ContentAuditActions.ResolveMutation(statusChanged: previousIsActive != collection.IsActive), ContentAuditEntityTypes.Collection, collection.Id, command.ActorUserId, Before: before, After: CreateCollectionCommandHandler.Snapshot(collection)), ct); await unitOfWork.SaveChangesAsync(ct); await cache.InvalidateSiteAsync(previousSiteKey, ct); if (!string.Equals(previousSiteKey, collection.SiteKey, StringComparison.OrdinalIgnoreCase)) await cache.InvalidateSiteAsync(collection.SiteKey, ct); return Result.Success();
    }
}

public sealed class DeleteCollectionCommandHandler(ICollectionRepository collections, IContentAuditService audit, IContentCacheService cache, IUnitOfWork unitOfWork) : ICommandHandler<DeleteCollectionCommand, Result>
{
    public async Task<Result> HandleAsync(DeleteCollectionCommand command, CancellationToken ct = default)
    {
        var collection = await collections.GetByIdAsync(command.Id, ct); if (collection is null || collection.IsDeleted) return Result.Failure(ContentErrors.CollectionNotFound); var siteKey = collection.SiteKey; var before = CreateCollectionCommandHandler.Snapshot(collection); collection.Delete(command.ActorUserId); await audit.PublishAsync(new ContentAuditEvent(ContentAuditEventTypes.CollectionDeleted, ContentAuditActions.Deleted, ContentAuditEntityTypes.Collection, collection.Id, command.ActorUserId, Before: before), ct); await unitOfWork.SaveChangesAsync(ct); await cache.InvalidateSiteAsync(siteKey, ct); return Result.Success();
    }
}
