using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Content.Application.Abstractions.Auditing;
using Dhole.Content.Application.Abstractions.Repositories;
using Dhole.Content.Application.Auditing;
using Dhole.Content.Domain.Collections.Entities;
using Dhole.Content.Domain.Shared;

namespace Dhole.Content.Application.Collections;

public sealed record CreateCollectionItemCommand(Guid CollectionId, string DataJson, int SortOrder, bool IsActive, Guid? ActorUserId) : ICommand<Result<Guid>>;
public sealed record UpdateCollectionItemCommand(Guid CollectionId, Guid ItemId, string DataJson, int SortOrder, bool IsActive, Guid? ActorUserId) : ICommand<Result>;
public sealed record DeleteCollectionItemCommand(Guid CollectionId, Guid ItemId, Guid? ActorUserId) : ICommand<Result>;

public sealed class CreateCollectionItemCommandHandler(
    ICollectionRepository collections,
    ICollectionItemRepository items,
    IContentAuditService audit,
    IUnitOfWork unitOfWork) : ICommandHandler<CreateCollectionItemCommand, Result<Guid>>
{
    public async Task<Result<Guid>> HandleAsync(CreateCollectionItemCommand command, CancellationToken cancellationToken = default)
    {
        var collection = await collections.GetByIdAsync(command.CollectionId, cancellationToken);
        if (collection is null || collection.IsDeleted) return Result.Failure<Guid>(ContentErrors.CollectionNotFound);

        ContentCollectionItem item;
        try
        {
            item = ContentCollectionItem.Create(command.CollectionId, command.DataJson, command.SortOrder,
                command.IsActive, command.ActorUserId);
        }
        catch (ArgumentException)
        {
            return Result.Failure<Guid>(ContentErrors.InvalidCollectionData);
        }

        await items.AddAsync(item, cancellationToken);
        await audit.PublishAsync(new ContentAuditEvent(ContentAuditEventTypes.CollectionItemCreated, ContentAuditActions.Created,
            ContentAuditEntityTypes.CollectionItem, item.Id, command.ActorUserId, After: Snapshot(item)), cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(item.Id);
    }

    internal static object Snapshot(ContentCollectionItem item) => new
    {
        item.Id, item.CollectionId, item.DataJson, item.SortOrder, item.IsActive
    };
}

public sealed class UpdateCollectionItemCommandHandler(
    ICollectionRepository collections,
    ICollectionItemRepository items,
    IContentAuditService audit,
    IUnitOfWork unitOfWork) : ICommandHandler<UpdateCollectionItemCommand, Result>
{
    public async Task<Result> HandleAsync(UpdateCollectionItemCommand command, CancellationToken cancellationToken = default)
    {
        var collection = await collections.GetByIdAsync(command.CollectionId, cancellationToken);
        if (collection is null || collection.IsDeleted) return Result.Failure(ContentErrors.CollectionNotFound);
        var item = await items.GetByIdAsync(command.ItemId, cancellationToken);
        if (item is null || item.IsDeleted || item.CollectionId != command.CollectionId)
            return Result.Failure(ContentErrors.CollectionItemNotFound);

        var before = CreateCollectionItemCommandHandler.Snapshot(item);
        try
        {
            item.Update(command.DataJson, command.SortOrder, command.IsActive, command.ActorUserId);
        }
        catch (ArgumentException)
        {
            return Result.Failure(ContentErrors.InvalidCollectionData);
        }

        await audit.PublishAsync(new ContentAuditEvent(ContentAuditEventTypes.CollectionItemUpdated, ContentAuditActions.Updated,
            ContentAuditEntityTypes.CollectionItem, item.Id, command.ActorUserId, Before: before,
            After: CreateCollectionItemCommandHandler.Snapshot(item)), cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public sealed class DeleteCollectionItemCommandHandler(
    ICollectionItemRepository items,
    IContentAuditService audit,
    IUnitOfWork unitOfWork) : ICommandHandler<DeleteCollectionItemCommand, Result>
{
    public async Task<Result> HandleAsync(DeleteCollectionItemCommand command, CancellationToken cancellationToken = default)
    {
        var item = await items.GetByIdAsync(command.ItemId, cancellationToken);
        if (item is null || item.IsDeleted || item.CollectionId != command.CollectionId)
            return Result.Failure(ContentErrors.CollectionItemNotFound);
        var before = CreateCollectionItemCommandHandler.Snapshot(item);
        item.Delete(command.ActorUserId);
        await audit.PublishAsync(new ContentAuditEvent(ContentAuditEventTypes.CollectionItemDeleted, ContentAuditActions.Deleted,
            ContentAuditEntityTypes.CollectionItem, item.Id, command.ActorUserId, Before: before), cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
