using CustomCodeFramework.Postgres.EntityFramework.Repositories;
using Dhole.Content.Application.Abstractions.Repositories;
using Dhole.Content.Domain.Placements.Entities;
using Dhole.Content.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Dhole.Content.Persistence.Repositories;

public sealed class PlacementItemRepository(ServiceDbContext db) : EfRepository<PlacementItem, Guid>(db), IPlacementItemRepository
{
    public Task<bool> ExistsAsync(Guid placementId, Guid contentId, Guid? excludeId = null, CancellationToken cancellationToken = default)
        => db.PlacementItems.AnyAsync(x => !x.IsDeleted && x.PlacementId == placementId && x.ContentId == contentId &&
            (!excludeId.HasValue || x.Id != excludeId.Value), cancellationToken);

    public async Task<IReadOnlyCollection<PlacementItem>> GetByPlacementAsync(Guid placementId, CancellationToken cancellationToken = default)
        => await db.PlacementItems.AsNoTracking()
            .Where(x => !x.IsDeleted && x.PlacementId == placementId)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);
}
