using CustomCodeFramework.Postgres.EntityFramework.Repositories;
using Dhole.Content.Application.Abstractions.Repositories;
using Dhole.Content.Domain.Meetings.Entities;
using Dhole.Content.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Dhole.Content.Persistence.Repositories;

public sealed class MeetingTypeRepository(ServiceDbContext db) : EfRepository<MeetingType, Guid>(db), IMeetingTypeRepository
{
    public Task<bool> ExistsBySlugAsync(string siteKey, string slug, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var normalizedSite = siteKey.Trim().ToLowerInvariant();
        var normalizedSlug = slug.Trim().ToLowerInvariant();
        return db.MeetingTypes.AnyAsync(x => !x.IsDeleted && x.SiteKey == normalizedSite && x.Slug == normalizedSlug &&
            (!excludeId.HasValue || x.Id != excludeId.Value), cancellationToken);
    }

    public async Task<IReadOnlyCollection<MeetingType>> GetAllAsync(string? siteKey = null, bool? isActive = null, CancellationToken cancellationToken = default)
    {
        var query = db.MeetingTypes.AsNoTracking().Where(x => !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(siteKey))
        {
            var normalized = siteKey.Trim().ToLowerInvariant();
            query = query.Where(x => x.SiteKey == normalized);
        }
        if (isActive.HasValue) query = query.Where(x => x.IsActive == isActive.Value);
        return await query.OrderBy(x => x.Name).ThenBy(x => x.Slug).ToListAsync(cancellationToken);
    }
}

public sealed class MeetingRequestRepository(ServiceDbContext db) : EfRepository<MeetingRequest, Guid>(db), IMeetingRequestRepository
{
    public async Task<IReadOnlyCollection<MeetingRequest>> GetAllAsync(Guid? meetingTypeId = null, string? status = null,
        Guid? assignedUserId = null, DateTime? fromUtc = null, DateTime? toUtc = null, CancellationToken cancellationToken = default)
    {
        var query = db.MeetingRequests.AsNoTracking().Where(x => !x.IsDeleted);
        if (meetingTypeId.HasValue) query = query.Where(x => x.MeetingTypeId == meetingTypeId.Value);
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(x => x.Status == status.Trim());
        if (assignedUserId.HasValue) query = query.Where(x => x.AssignedUserId == assignedUserId.Value);
        if (fromUtc.HasValue) query = query.Where(x => x.RequestedStartUtc >= fromUtc.Value);
        if (toUtc.HasValue) query = query.Where(x => x.RequestedStartUtc <= toUtc.Value);
        return await query.OrderBy(x => x.RequestedStartUtc).ThenBy(x => x.CreatedAtUtc).ToListAsync(cancellationToken);
    }
}
