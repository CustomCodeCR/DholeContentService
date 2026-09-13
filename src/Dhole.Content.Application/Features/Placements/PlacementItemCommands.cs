using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Content.Application.Abstractions.Auditing;
using Dhole.Content.Application.Abstractions.Cache;
using Dhole.Content.Application.Abstractions.Repositories;
using Dhole.Content.Application.Auditing;
using Dhole.Content.Domain.Placements.Entities;
using Dhole.Content.Domain.Shared;

namespace Dhole.Content.Application.Placements;

public sealed record CreatePlacementItemCommand(Guid PlacementId, Guid ContentId, int SortOrder, DateTime? ValidFromUtc, DateTime? ValidToUtc, string? SettingsJson, bool IsActive, Guid? ActorUserId) : ICommand<Result<Guid>>;
public sealed record UpdatePlacementItemCommand(Guid PlacementId, Guid ItemId, int SortOrder, DateTime? ValidFromUtc, DateTime? ValidToUtc, string? SettingsJson, bool IsActive, Guid? ActorUserId) : ICommand<Result>;
public sealed record DeletePlacementItemCommand(Guid PlacementId, Guid ItemId, Guid? ActorUserId) : ICommand<Result>;

public sealed class CreatePlacementItemCommandHandler(IPlacementRepository placements, IPlacementItemRepository items, IContentItemRepository contents, IContentAuditService audit, IContentCacheService cache, IUnitOfWork unitOfWork) : ICommandHandler<CreatePlacementItemCommand, Result<Guid>>
{
    public async Task<Result<Guid>> HandleAsync(CreatePlacementItemCommand command, CancellationToken ct = default)
    {
        var placement = await placements.GetByIdAsync(command.PlacementId, ct); if (placement is null || placement.IsDeleted) return Result.Failure<Guid>(ContentErrors.PlacementNotFound);
        var content = await contents.GetByIdAsync(command.ContentId, ct); if (content is null || content.IsDeleted) return Result.Failure<Guid>(ContentErrors.ContentNotFound);
        if (!string.Equals(content.SiteKey, placement.SiteKey, StringComparison.OrdinalIgnoreCase) || !placement.Allows(content.Type)) return Result.Failure<Guid>(ContentErrors.PlacementContentNotAllowed);
        if (await items.ExistsAsync(command.PlacementId, command.ContentId, null, ct)) return Result.Failure<Guid>(ContentErrors.PlacementItemAlreadyExists);
        PlacementItem item; try { item = PlacementItem.Create(command.PlacementId, command.ContentId, command.SortOrder, command.ValidFromUtc, command.ValidToUtc, command.SettingsJson, command.IsActive, command.ActorUserId); } catch (ArgumentException) { return Result.Failure<Guid>(ContentErrors.InvalidPlacementData); }
        await items.AddAsync(item, ct); await audit.PublishAsync(new ContentAuditEvent(ContentAuditEventTypes.PlacementItemCreated, ContentAuditActions.Created, ContentAuditEntityTypes.PlacementItem, item.Id, command.ActorUserId, After: Snapshot(item)), ct); await unitOfWork.SaveChangesAsync(ct); await cache.InvalidateSiteAsync(placement.SiteKey, ct); return Result.Success(item.Id);
    }
    internal static object Snapshot(PlacementItem item) => new { item.Id, item.PlacementId, item.ContentId, item.SortOrder, item.ValidFromUtc, item.ValidToUtc, item.SettingsJson, item.IsActive };
}

public sealed class UpdatePlacementItemCommandHandler(IPlacementRepository placements, IPlacementItemRepository items, IContentItemRepository contents, IContentAuditService audit, IContentCacheService cache, IUnitOfWork unitOfWork) : ICommandHandler<UpdatePlacementItemCommand, Result>
{
    public async Task<Result> HandleAsync(UpdatePlacementItemCommand command, CancellationToken ct = default)
    {
        var placement = await placements.GetByIdAsync(command.PlacementId, ct); if (placement is null || placement.IsDeleted) return Result.Failure(ContentErrors.PlacementNotFound);
        var item = await items.GetByIdAsync(command.ItemId, ct); if (item is null || item.IsDeleted || item.PlacementId != command.PlacementId) return Result.Failure(ContentErrors.PlacementItemNotFound);
        var content = await contents.GetByIdAsync(item.ContentId, ct); if (content is null || content.IsDeleted) return Result.Failure(ContentErrors.ContentNotFound);
        if (!string.Equals(content.SiteKey, placement.SiteKey, StringComparison.OrdinalIgnoreCase) || !placement.Allows(content.Type)) return Result.Failure(ContentErrors.PlacementContentNotAllowed);
        var before = CreatePlacementItemCommandHandler.Snapshot(item); var previousSortOrder = item.SortOrder; var previousIsActive = item.IsActive;
        try { item.Update(command.SortOrder, command.ValidFromUtc, command.ValidToUtc, command.SettingsJson, command.IsActive, command.ActorUserId); } catch (ArgumentException) { return Result.Failure(ContentErrors.InvalidPlacementData); }
        await audit.PublishAsync(new ContentAuditEvent(ContentAuditEventTypes.PlacementItemUpdated, ContentAuditActions.ResolveMutation(statusChanged: previousIsActive != item.IsActive, reordered: previousSortOrder != item.SortOrder), ContentAuditEntityTypes.PlacementItem, item.Id, command.ActorUserId, Before: before, After: CreatePlacementItemCommandHandler.Snapshot(item)), ct); await unitOfWork.SaveChangesAsync(ct); await cache.InvalidateSiteAsync(placement.SiteKey, ct); return Result.Success();
    }
}

public sealed class DeletePlacementItemCommandHandler(IPlacementRepository placements, IPlacementItemRepository items, IContentAuditService audit, IContentCacheService cache, IUnitOfWork unitOfWork) : ICommandHandler<DeletePlacementItemCommand, Result>
{
    public async Task<Result> HandleAsync(DeletePlacementItemCommand command, CancellationToken ct = default)
    {
        var placement = await placements.GetByIdAsync(command.PlacementId, ct); if (placement is null || placement.IsDeleted) return Result.Failure(ContentErrors.PlacementNotFound);
        var item = await items.GetByIdAsync(command.ItemId, ct); if (item is null || item.IsDeleted || item.PlacementId != command.PlacementId) return Result.Failure(ContentErrors.PlacementItemNotFound); var before = CreatePlacementItemCommandHandler.Snapshot(item); item.Delete(command.ActorUserId); await audit.PublishAsync(new ContentAuditEvent(ContentAuditEventTypes.PlacementItemDeleted, ContentAuditActions.Deleted, ContentAuditEntityTypes.PlacementItem, item.Id, command.ActorUserId, Before: before), ct); await unitOfWork.SaveChangesAsync(ct); await cache.InvalidateSiteAsync(placement.SiteKey, ct); return Result.Success();
    }
}
