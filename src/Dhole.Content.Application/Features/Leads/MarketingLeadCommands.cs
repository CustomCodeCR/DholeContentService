using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Content.Application.Abstractions.Auditing;
using Dhole.Content.Application.Abstractions.Repositories;
using Dhole.Content.Application.Auditing;
using Dhole.Content.Domain.Leads.Entities;
using Dhole.Content.Domain.Shared;

namespace Dhole.Content.Application.Leads;

public sealed record CreateMarketingLeadCommand(
    string SiteKey,
    string? FirstName,
    string? LastName,
    string? Email,
    string? Phone,
    string? Company,
    string? JobTitle,
    string? Country,
    string? Source,
    string? Status,
    Guid? OwnerUserId,
    DateTime? FirstTouchAtUtc,
    DateTime? LastTouchAtUtc,
    Guid? ActorUserId) : ICommand<Result<Guid>>;

public sealed record UpdateMarketingLeadCommand(
    Guid Id,
    string SiteKey,
    string? FirstName,
    string? LastName,
    string? Email,
    string? Phone,
    string? Company,
    string? JobTitle,
    string? Country,
    string? Source,
    string? Status,
    Guid? OwnerUserId,
    DateTime? LastTouchAtUtc,
    Guid? ActorUserId) : ICommand<Result>;

public sealed record TouchMarketingLeadCommand(Guid Id, string? Source, Guid? ActorUserId) : ICommand<Result>;
public sealed record DeleteMarketingLeadCommand(Guid Id, Guid? ActorUserId) : ICommand<Result>;

public sealed class CreateMarketingLeadCommandHandler(
    ISiteRepository sites,
    IMarketingLeadRepository leads,
    IContentAuditService audit,
    IUnitOfWork unitOfWork) : ICommandHandler<CreateMarketingLeadCommand, Result<Guid>>
{
    public async Task<Result<Guid>> HandleAsync(CreateMarketingLeadCommand command, CancellationToken cancellationToken = default)
    {
        var site = await sites.GetBySiteKeyAsync(command.SiteKey, cancellationToken);
        if (site is null || site.IsDeleted) return Result.Failure<Guid>(ContentErrors.SiteNotFound);
        if (!string.IsNullOrWhiteSpace(command.Email) &&
            await leads.ExistsByEmailAsync(command.SiteKey, command.Email, null, cancellationToken))
            return Result.Failure<Guid>(ContentErrors.MarketingLeadEmailAlreadyExists);

        MarketingLead lead;
        try
        {
            lead = MarketingLead.Create(command.SiteKey, command.FirstName, command.LastName, command.Email,
                command.Phone, command.Company, command.JobTitle, command.Country, command.Source, command.Status,
                command.OwnerUserId, command.FirstTouchAtUtc, command.LastTouchAtUtc, command.ActorUserId, DateTime.UtcNow);
        }
        catch (ArgumentException)
        {
            return Result.Failure<Guid>(ContentErrors.InvalidMarketingLeadData);
        }

        await leads.AddAsync(lead, cancellationToken);
        await audit.PublishAsync(new ContentAuditEvent(ContentAuditEventTypes.MarketingLeadCreated,
            ContentAuditActions.Created, ContentAuditEntityTypes.MarketingLead, lead.Id, command.ActorUserId,
            After: Snapshot(lead)), cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(lead.Id);
    }

    internal static object Snapshot(MarketingLead lead) => new
    {
        lead.Id,
        lead.SiteKey,
        lead.Source,
        lead.Status,
        lead.OwnerUserId,
        lead.FirstTouchAtUtc,
        lead.LastTouchAtUtc
    };
}

public sealed class UpdateMarketingLeadCommandHandler(
    ISiteRepository sites,
    IMarketingLeadRepository leads,
    IContentAuditService audit,
    IUnitOfWork unitOfWork) : ICommandHandler<UpdateMarketingLeadCommand, Result>
{
    public async Task<Result> HandleAsync(UpdateMarketingLeadCommand command, CancellationToken cancellationToken = default)
    {
        var lead = await leads.GetByIdAsync(command.Id, cancellationToken);
        if (lead is null || lead.IsDeleted) return Result.Failure(ContentErrors.MarketingLeadNotFound);
        var site = await sites.GetBySiteKeyAsync(command.SiteKey, cancellationToken);
        if (site is null || site.IsDeleted) return Result.Failure(ContentErrors.SiteNotFound);
        if (!string.IsNullOrWhiteSpace(command.Email) &&
            await leads.ExistsByEmailAsync(command.SiteKey, command.Email, command.Id, cancellationToken))
            return Result.Failure(ContentErrors.MarketingLeadEmailAlreadyExists);

        var before = CreateMarketingLeadCommandHandler.Snapshot(lead);
        try
        {
            lead.Update(command.SiteKey, command.FirstName, command.LastName, command.Email, command.Phone,
                command.Company, command.JobTitle, command.Country, command.Source, command.Status,
                command.OwnerUserId, command.LastTouchAtUtc, command.ActorUserId, DateTime.UtcNow);
        }
        catch (ArgumentException)
        {
            return Result.Failure(ContentErrors.InvalidMarketingLeadData);
        }

        await audit.PublishAsync(new ContentAuditEvent(ContentAuditEventTypes.MarketingLeadUpdated,
            ContentAuditActions.Updated, ContentAuditEntityTypes.MarketingLead, lead.Id, command.ActorUserId,
            Before: before, After: CreateMarketingLeadCommandHandler.Snapshot(lead)), cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public sealed class TouchMarketingLeadCommandHandler(
    IMarketingLeadRepository leads,
    IContentAuditService audit,
    IUnitOfWork unitOfWork) : ICommandHandler<TouchMarketingLeadCommand, Result>
{
    public async Task<Result> HandleAsync(TouchMarketingLeadCommand command, CancellationToken cancellationToken = default)
    {
        var lead = await leads.GetByIdAsync(command.Id, cancellationToken);
        if (lead is null || lead.IsDeleted) return Result.Failure(ContentErrors.MarketingLeadNotFound);
        var before = CreateMarketingLeadCommandHandler.Snapshot(lead);
        try
        {
            lead.Touch(command.Source, DateTime.UtcNow, command.ActorUserId);
        }
        catch (ArgumentException)
        {
            return Result.Failure(ContentErrors.InvalidMarketingLeadData);
        }
        await audit.PublishAsync(new ContentAuditEvent(ContentAuditEventTypes.MarketingLeadUpdated,
            ContentAuditActions.Updated, ContentAuditEntityTypes.MarketingLead, lead.Id, command.ActorUserId,
            Before: before, After: CreateMarketingLeadCommandHandler.Snapshot(lead)), cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public sealed class DeleteMarketingLeadCommandHandler(
    IMarketingLeadRepository leads,
    IContentAuditService audit,
    IUnitOfWork unitOfWork) : ICommandHandler<DeleteMarketingLeadCommand, Result>
{
    public async Task<Result> HandleAsync(DeleteMarketingLeadCommand command, CancellationToken cancellationToken = default)
    {
        var lead = await leads.GetByIdAsync(command.Id, cancellationToken);
        if (lead is null || lead.IsDeleted) return Result.Failure(ContentErrors.MarketingLeadNotFound);
        var before = CreateMarketingLeadCommandHandler.Snapshot(lead);
        lead.Delete(command.ActorUserId, DateTime.UtcNow);
        await audit.PublishAsync(new ContentAuditEvent(ContentAuditEventTypes.MarketingLeadDeleted,
            ContentAuditActions.Deleted, ContentAuditEntityTypes.MarketingLead, lead.Id, command.ActorUserId,
            Before: before), cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
