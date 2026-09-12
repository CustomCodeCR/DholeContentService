using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Content.Domain.Meetings.Entities;

namespace Dhole.Content.Application.Abstractions.Repositories;

public interface IMeetingTypeRepository : IRepository<MeetingType, Guid>
{
    Task<bool> ExistsBySlugAsync(string siteKey, string slug, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<MeetingType>> GetAllAsync(string? siteKey = null, bool? isActive = null, CancellationToken cancellationToken = default);
}

public interface IMeetingRequestRepository : IRepository<MeetingRequest, Guid>
{
    Task<IReadOnlyCollection<MeetingRequest>> GetAllAsync(Guid? meetingTypeId = null, string? status = null,
        Guid? assignedUserId = null, DateTime? fromUtc = null, DateTime? toUtc = null,
        CancellationToken cancellationToken = default);
}
