using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Queries;
using Dhole.Content.Application.Abstractions.Repositories;
using Dhole.Content.Contracts.Meetings;
using Dhole.Content.Domain.Meetings.Entities;
using Dhole.Content.Domain.Shared;

namespace Dhole.Content.Application.Meetings;

public sealed record GetMeetingTypesQuery(string? SiteKey, bool? IsActive) : IQuery<IReadOnlyCollection<MeetingTypeDto>>;
public sealed record GetMeetingTypeByIdQuery(Guid Id) : IQuery<Result<MeetingTypeDto>>;
public sealed record GetMeetingRequestsQuery(Guid? MeetingTypeId, string? Status, Guid? AssignedUserId, DateTime? FromUtc, DateTime? ToUtc) : IQuery<IReadOnlyCollection<MeetingRequestDto>>;
public sealed record GetMeetingRequestByIdQuery(Guid Id) : IQuery<Result<MeetingRequestDto>>;

public sealed class GetMeetingTypesQueryHandler(IMeetingTypeRepository repository)
    : IQueryHandler<GetMeetingTypesQuery, IReadOnlyCollection<MeetingTypeDto>>
{
    public async Task<IReadOnlyCollection<MeetingTypeDto>> HandleAsync(GetMeetingTypesQuery query, CancellationToken cancellationToken = default)
        => (await repository.GetAllAsync(query.SiteKey, query.IsActive, cancellationToken)).Select(Map).ToArray();

    internal static MeetingTypeDto Map(MeetingType item) => new(item.Id, item.SiteKey, item.Name, item.Slug,
        item.Description, item.DurationMinutes, item.BufferMinutes, item.MeetingMode, item.AssignedUserId,
        item.AssignedTeamKey, item.SettingsJson, item.IsActive, item.CreatedAtUtc, item.UpdatedAtUtc);
}

public sealed class GetMeetingTypeByIdQueryHandler(IMeetingTypeRepository repository)
    : IQueryHandler<GetMeetingTypeByIdQuery, Result<MeetingTypeDto>>
{
    public async Task<Result<MeetingTypeDto>> HandleAsync(GetMeetingTypeByIdQuery query, CancellationToken cancellationToken = default)
    {
        var item = await repository.GetByIdAsync(query.Id, cancellationToken);
        return item is null || item.IsDeleted
            ? Result.Failure<MeetingTypeDto>(ContentErrors.MeetingTypeNotFound)
            : Result.Success(GetMeetingTypesQueryHandler.Map(item));
    }
}

public sealed class GetMeetingRequestsQueryHandler(IMeetingRequestRepository repository)
    : IQueryHandler<GetMeetingRequestsQuery, IReadOnlyCollection<MeetingRequestDto>>
{
    public async Task<IReadOnlyCollection<MeetingRequestDto>> HandleAsync(GetMeetingRequestsQuery query, CancellationToken cancellationToken = default)
        => (await repository.GetAllAsync(query.MeetingTypeId, query.Status, query.AssignedUserId, query.FromUtc, query.ToUtc, cancellationToken))
            .Select(Map).ToArray();

    internal static MeetingRequestDto Map(MeetingRequest item) => new(item.Id, item.MeetingTypeId, item.LeadId,
        item.SubmissionId, item.RequestedStartUtc, item.RequestedEndUtc, item.TimeZone, item.Subject, item.Message,
        item.Status, item.AssignedUserId, item.ConfirmedStartUtc, item.ConfirmedEndUtc, item.ExternalProvider,
        item.ExternalEventId, item.MeetingUrl, item.CreatedAtUtc, item.UpdatedAtUtc);
}

public sealed class GetMeetingRequestByIdQueryHandler(IMeetingRequestRepository repository)
    : IQueryHandler<GetMeetingRequestByIdQuery, Result<MeetingRequestDto>>
{
    public async Task<Result<MeetingRequestDto>> HandleAsync(GetMeetingRequestByIdQuery query, CancellationToken cancellationToken = default)
    {
        var item = await repository.GetByIdAsync(query.Id, cancellationToken);
        return item is null || item.IsDeleted
            ? Result.Failure<MeetingRequestDto>(ContentErrors.MeetingRequestNotFound)
            : Result.Success(GetMeetingRequestsQueryHandler.Map(item));
    }
}
