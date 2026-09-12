using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Content.Application.Abstractions.Auditing;
using Dhole.Content.Application.Abstractions.Repositories;
using Dhole.Content.Application.Auditing;
using Dhole.Content.Domain.Placements.Entities;
using Dhole.Content.Domain.Shared;

namespace Dhole.Content.Application.Placements;

public sealed record CreatePlacementCommand(string SiteKey, string Code, string Name, string AllowedTypesJson, int MaxItems, string? SettingsJson, bool IsActive, Guid? ActorUserId) : ICommand<Result<Guid>>;
public sealed record UpdatePlacementCommand(Guid Id, string SiteKey, string Code, string Name, string AllowedTypesJson, int MaxItems, string? SettingsJson, bool IsActive, Guid? ActorUserId) : ICommand<Result>;
public sealed record DeletePlacementCommand(Guid Id, Guid? ActorUserId) : ICommand<Result>;

public sealed class CreatePlacementCommandHandler(
    ISiteRepository sites,
    IPlacementRepository placements,
    IContentAuditService audit,
    IUnitOfWork unitOfWork) : ICommandHandler<CreatePlacementCommand, Result<Guid>>
{
    public async Task<Result<Guid>> HandleAsync(CreatePlacementCommand command, CancellationToken cancellationToken = default)
    {
        var site = await sites.GetBySiteKeyAsync(command.SiteKey, cancellationToken);
        if (site is null || site.IsDeleted) return Result.Failure<Guid>(ContentErrors.SiteNotFound);
        if (await placements.ExistsByCodeAsync(command.SiteKey, command.Code, null, cancellationToken))
            return Result.Failure<Guid>(ContentErrors.PlacementCodeAlreadyExists);

        Placement placement;
        try
        {
            placement = Placement.Create(command.SiteKey, command.Code, command.Name, command.AllowedTypesJson,
                command.MaxItems, command.SettingsJson, command.IsActive, command.ActorUserId);
        }
        catch (ArgumentException)
        {
            return Result.Failure<Guid>(ContentErrors.InvalidPlacementData);
        }

        await placements.AddAsync(placement, cancellationToken);
        await audit.PublishAsync(new ContentAuditEvent(ContentAuditEventTypes.PlacementCreated, ContentAuditActions.Created,
            ContentAuditEntityTypes.Placement, placement.Id, command.ActorUserId, After: Snapshot(placement)), cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(placement.Id);
    }

    internal static object Snapshot(Placement placement) => new
    {
        placement.Id, placement.SiteKey, placement.Code, placement.Name, placement.AllowedTypesJson,
        placement.MaxItems, placement.SettingsJson, placement.IsActive
    };
}

public sealed class UpdatePlacementCommandHandler(
    ISiteRepository sites,
    IPlacementRepository placements,
    IContentAuditService audit,
    IUnitOfWork unitOfWork) : ICommandHandler<UpdatePlacementCommand, Result>
{
    public async Task<Result> HandleAsync(UpdatePlacementCommand command, CancellationToken cancellationToken = default)
    {
        var placement = await placements.GetByIdAsync(command.Id, cancellationToken);
        if (placement is null || placement.IsDeleted) return Result.Failure(ContentErrors.PlacementNotFound);
        var site = await sites.GetBySiteKeyAsync(command.SiteKey, cancellationToken);
        if (site is null || site.IsDeleted) return Result.Failure(ContentErrors.SiteNotFound);
        if (await placements.ExistsByCodeAsync(command.SiteKey, command.Code, command.Id, cancellationToken))
            return Result.Failure(ContentErrors.PlacementCodeAlreadyExists);

        var before = CreatePlacementCommandHandler.Snapshot(placement);
        try
        {
            placement.Update(command.SiteKey, command.Code, command.Name, command.AllowedTypesJson,
                command.MaxItems, command.SettingsJson, command.IsActive, command.ActorUserId);
        }
        catch (ArgumentException)
        {
            return Result.Failure(ContentErrors.InvalidPlacementData);
        }

        await audit.PublishAsync(new ContentAuditEvent(ContentAuditEventTypes.PlacementUpdated, ContentAuditActions.Updated,
            ContentAuditEntityTypes.Placement, placement.Id, command.ActorUserId, Before: before,
            After: CreatePlacementCommandHandler.Snapshot(placement)), cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public sealed class DeletePlacementCommandHandler(
    IPlacementRepository placements,
    IContentAuditService audit,
    IUnitOfWork unitOfWork) : ICommandHandler<DeletePlacementCommand, Result>
{
    public async Task<Result> HandleAsync(DeletePlacementCommand command, CancellationToken cancellationToken = default)
    {
        var placement = await placements.GetByIdAsync(command.Id, cancellationToken);
        if (placement is null || placement.IsDeleted) return Result.Failure(ContentErrors.PlacementNotFound);
        var before = CreatePlacementCommandHandler.Snapshot(placement);
        placement.Delete(command.ActorUserId);
        await audit.PublishAsync(new ContentAuditEvent(ContentAuditEventTypes.PlacementDeleted, ContentAuditActions.Deleted,
            ContentAuditEntityTypes.Placement, placement.Id, command.ActorUserId, Before: before), cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
