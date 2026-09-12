using CustomCodeFramework.Postgres.EntityFramework.Repositories;
using Dhole.Content.Application.Abstractions.Repositories;
using Dhole.Content.Domain.Placements;
using Dhole.Content.Domain.Placements.Entities;
using Dhole.Content.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Dhole.Content.Persistence.Repositories;

public sealed class PlacementRepository(ServiceDbContext db) : EfRepository<Placement, Guid>(db), IPlacementRepository
{
    public Task<bool> ExistsByCodeAsync(string siteKey, string code, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var normalizedSite = siteKey.Trim().ToLowerInvariant();
        var normalizedCode = PlacementRules.NormalizeCode(code);
        return db.Placements.AnyAsync(x => !x.IsDeleted && x.SiteKey == normalizedSite && x.Code == normalizedCode &&
            (!excludeId.HasValue || x.Id != excludeId.Value), cancellationToken);
    }

    public async Task<IReadOnlyCollection<Placement>> GetAllAsync(string? siteKey = null, CancellationToken cancellationToken = default)
    {
        var query = db.Placements.AsNoTracking().Where(x => !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(siteKey))
        {
            var normalizedSite = siteKey.Trim().ToLowerInvariant();
            query = query.Where(x => x.SiteKey == normalizedSite);
        }
        return await query.OrderBy(x => x.SiteKey).ThenBy(x => x.Code).ToListAsync(cancellationToken);
    }
}
