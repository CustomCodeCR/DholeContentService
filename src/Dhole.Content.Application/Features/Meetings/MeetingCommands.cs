using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Content.Application.Abstractions.Repositories;
using Dhole.Content.Domain.Meetings.Entities;
using Dhole.Content.Domain.Shared;

namespace Dhole.Content.Application.Meetings;

public sealed record CreateMeetingTypeCommand(string SiteKey, string Name, string Slug, string? Description,
    int DurationMinutes, int BufferMinutes, string MeetingMode, Guid? AssignedUserId, string? AssignedTeamKey,
    string? SettingsJson, bool IsActive, Guid? ActorUserId) : ICommand<Result<Guid>>;
public sealed record UpdateMeetingTypeCommand(Guid Id, string SiteKey, string Name, string Slug, string? Description,
    int DurationMinutes, int BufferMinutes, string MeetingMode, Guid? AssignedUserId, string? AssignedTeamKey,
    string? SettingsJson, bool IsActive, Guid? ActorUserId) : ICommand<Result>;
public sealed record DeleteMeetingTypeCommand(Guid Id, Guid? ActorUserId) : ICommand<Result>;
public sealed record CreateMeetingRequestCommand(Guid MeetingTypeId, Guid? LeadId, Guid? SubmissionId,
    DateTime RequestedStartUtc, DateTime? RequestedEndUtc, string TimeZone, string Subject, string? Message,
    Guid? ActorUserId) : ICommand<Result<Guid>>;
public sealed record MarkMeetingPendingCommand(Guid Id, Guid? AssignedUserId, Guid? ActorUserId) : ICommand<Result>;
public sealed record ConfirmMeetingCommand(Guid Id, DateTime ConfirmedStartUtc, DateTime ConfirmedEndUtc,
    Guid? AssignedUserId, string? ExternalProvider, string? ExternalEventId, string? MeetingUrl, Guid? ActorUserId) : ICommand<Result>;
public sealed record RejectMeetingCommand(Guid Id, Guid? ActorUserId) : ICommand<Result>;
public sealed record CancelMeetingCommand(Guid Id, Guid? ActorUserId) : ICommand<Result>;
public sealed record CompleteMeetingCommand(Guid Id, Guid? ActorUserId) : ICommand<Result>;

public sealed class CreateMeetingTypeCommandHandler(ISiteRepository sites, IMeetingTypeRepository meetingTypes, IUnitOfWork unitOfWork)
    : ICommandHandler<CreateMeetingTypeCommand, Result<Guid>>
{
    public async Task<Result<Guid>> HandleAsync(CreateMeetingTypeCommand command, CancellationToken cancellationToken = default)
    {
        var site = await sites.GetBySiteKeyAsync(command.SiteKey, cancellationToken);
        if (site is null || site.IsDeleted) return Result.Failure<Guid>(ContentErrors.SiteNotFound);
        if (await meetingTypes.ExistsBySlugAsync(command.SiteKey, command.Slug, null, cancellationToken))
            return Result.Failure<Guid>(ContentErrors.MeetingTypeSlugAlreadyExists);
        try
        {
            var item = MeetingType.Create(command.SiteKey, command.Name, command.Slug, command.Description,
                command.DurationMinutes, command.BufferMinutes, command.MeetingMode, command.AssignedUserId,
                command.AssignedTeamKey, command.SettingsJson, command.IsActive, command.ActorUserId);
            await meetingTypes.AddAsync(item, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success(item.Id);
        }
        catch (ArgumentException) { return Result.Failure<Guid>(ContentErrors.InvalidMeetingData); }
    }
}

public sealed class UpdateMeetingTypeCommandHandler(ISiteRepository sites, IMeetingTypeRepository meetingTypes, IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateMeetingTypeCommand, Result>
{
    public async Task<Result> HandleAsync(UpdateMeetingTypeCommand command, CancellationToken cancellationToken = default)
    {
        var item = await meetingTypes.GetByIdAsync(command.Id, cancellationToken);
        if (item is null || item.IsDeleted) return Result.Failure(ContentErrors.MeetingTypeNotFound);
        var site = await sites.GetBySiteKeyAsync(command.SiteKey, cancellationToken);
        if (site is null || site.IsDeleted) return Result.Failure(ContentErrors.SiteNotFound);
        if (await meetingTypes.ExistsBySlugAsync(command.SiteKey, command.Slug, command.Id, cancellationToken))
            return Result.Failure(ContentErrors.MeetingTypeSlugAlreadyExists);
        try
        {
            item.Update(command.SiteKey, command.Name, command.Slug, command.Description, command.DurationMinutes,
                command.BufferMinutes, command.MeetingMode, command.AssignedUserId, command.AssignedTeamKey,
                command.SettingsJson, command.IsActive, command.ActorUserId);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (ArgumentException) { return Result.Failure(ContentErrors.InvalidMeetingData); }
    }
}

public sealed class DeleteMeetingTypeCommandHandler(IMeetingTypeRepository meetingTypes, IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteMeetingTypeCommand, Result>
{
    public async Task<Result> HandleAsync(DeleteMeetingTypeCommand command, CancellationToken cancellationToken = default)
    {
        var item = await meetingTypes.GetByIdAsync(command.Id, cancellationToken);
        if (item is null || item.IsDeleted) return Result.Failure(ContentErrors.MeetingTypeNotFound);
        item.Delete(command.ActorUserId);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public sealed class CreateMeetingRequestCommandHandler(IMeetingTypeRepository meetingTypes, IMarketingLeadRepository leads,
    IMarketingSubmissionRepository submissions, IMarketingFormRepository forms, IMeetingRequestRepository requests, IUnitOfWork unitOfWork)
    : ICommandHandler<CreateMeetingRequestCommand, Result<Guid>>
{
    public async Task<Result<Guid>> HandleAsync(CreateMeetingRequestCommand command, CancellationToken cancellationToken = default)
    {
        var type = await meetingTypes.GetByIdAsync(command.MeetingTypeId, cancellationToken);
        if (type is null || type.IsDeleted || !type.IsActive) return Result.Failure<Guid>(ContentErrors.MeetingTypeNotFound);
        if (!command.LeadId.HasValue && !command.SubmissionId.HasValue)
            return Result.Failure<Guid>(ContentErrors.InvalidMeetingData);

        if (command.LeadId.HasValue)
        {
            var lead = await leads.GetByIdAsync(command.LeadId.Value, cancellationToken);
            if (lead is null || lead.IsDeleted) return Result.Failure<Guid>(ContentErrors.MarketingLeadNotFound);
            if (!string.Equals(lead.SiteKey, type.SiteKey, StringComparison.Ordinal))
                return Result.Failure<Guid>(ContentErrors.InvalidMeetingData);
        }

        if (command.SubmissionId.HasValue)
        {
            var submission = await submissions.GetByIdAsync(command.SubmissionId.Value, cancellationToken);
            if (submission is null || submission.IsDeleted) return Result.Failure<Guid>(ContentErrors.MarketingSubmissionNotFound);
            var form = await forms.GetByIdAsync(submission.FormId, cancellationToken);
            if (form is null || form.IsDeleted || !string.Equals(form.SiteKey, type.SiteKey, StringComparison.Ordinal))
                return Result.Failure<Guid>(ContentErrors.InvalidMeetingData);
        }

        try
        {
            var requestedEnd = command.RequestedEndUtc ?? command.RequestedStartUtc.AddMinutes(type.DurationMinutes);
            var request = MeetingRequest.Create(type.Id, command.LeadId, command.SubmissionId, command.RequestedStartUtc,
                requestedEnd, command.TimeZone, command.Subject, command.Message, type.AssignedUserId, command.ActorUserId);
            await requests.AddAsync(request, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success(request.Id);
        }
        catch (ArgumentException) { return Result.Failure<Guid>(ContentErrors.InvalidMeetingData); }
    }
}

internal static class MeetingRequestCommandHelpers
{
    public static async Task<Result> ApplyAsync(IMeetingRequestRepository requests, IUnitOfWork unitOfWork, Guid id,
        Action<MeetingRequest> action, CancellationToken cancellationToken)
    {
        var request = await requests.GetByIdAsync(id, cancellationToken);
        if (request is null || request.IsDeleted) return Result.Failure(ContentErrors.MeetingRequestNotFound);
        try { action(request); }
        catch (InvalidOperationException) { return Result.Failure(ContentErrors.InvalidMeetingState); }
        catch (ArgumentException) { return Result.Failure(ContentErrors.InvalidMeetingData); }
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public sealed class MarkMeetingPendingCommandHandler(IMeetingRequestRepository requests, IUnitOfWork unitOfWork)
    : ICommandHandler<MarkMeetingPendingCommand, Result>
{
    public Task<Result> HandleAsync(MarkMeetingPendingCommand command, CancellationToken cancellationToken = default)
        => MeetingRequestCommandHelpers.ApplyAsync(requests, unitOfWork, command.Id,
            item => item.MarkPendingConfirmation(command.AssignedUserId, command.ActorUserId), cancellationToken);
}

public sealed class ConfirmMeetingCommandHandler(IMeetingRequestRepository requests, IUnitOfWork unitOfWork)
    : ICommandHandler<ConfirmMeetingCommand, Result>
{
    public Task<Result> HandleAsync(ConfirmMeetingCommand command, CancellationToken cancellationToken = default)
        => MeetingRequestCommandHelpers.ApplyAsync(requests, unitOfWork, command.Id,
            item => item.Confirm(command.ConfirmedStartUtc, command.ConfirmedEndUtc, command.AssignedUserId,
                command.ExternalProvider, command.ExternalEventId, command.MeetingUrl, command.ActorUserId), cancellationToken);
}

public sealed class RejectMeetingCommandHandler(IMeetingRequestRepository requests, IUnitOfWork unitOfWork)
    : ICommandHandler<RejectMeetingCommand, Result>
{
    public Task<Result> HandleAsync(RejectMeetingCommand command, CancellationToken cancellationToken = default)
        => MeetingRequestCommandHelpers.ApplyAsync(requests, unitOfWork, command.Id, item => item.Reject(command.ActorUserId), cancellationToken);
}
public sealed class CancelMeetingCommandHandler(IMeetingRequestRepository requests, IUnitOfWork unitOfWork)
    : ICommandHandler<CancelMeetingCommand, Result>
{
    public Task<Result> HandleAsync(CancelMeetingCommand command, CancellationToken cancellationToken = default)
        => MeetingRequestCommandHelpers.ApplyAsync(requests, unitOfWork, command.Id, item => item.Cancel(command.ActorUserId), cancellationToken);
}
public sealed class CompleteMeetingCommandHandler(IMeetingRequestRepository requests, IUnitOfWork unitOfWork)
    : ICommandHandler<CompleteMeetingCommand, Result>
{
    public Task<Result> HandleAsync(CompleteMeetingCommand command, CancellationToken cancellationToken = default)
        => MeetingRequestCommandHelpers.ApplyAsync(requests, unitOfWork, command.Id, item => item.Complete(command.ActorUserId), cancellationToken);
}
