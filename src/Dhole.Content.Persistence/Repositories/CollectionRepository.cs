using CustomCodeFramework.Postgres.EntityFramework.Repositories;
using Dhole.Content.Application.Abstractions.Repositories;
using Dhole.Content.Domain.Collections;
using Dhole.Content.Domain.Collections.Entities;
using Dhole.Content.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Dhole.Content.Persistence.Repositories;

public sealed class CollectionRepository(ServiceDbContext db) : EfRepository<ContentCollection, Guid>(db), ICollectionRepository
{
    public Task<bool> ExistsByCodeAsync(string siteKey, string code, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var normalizedSite = siteKey.Trim().ToLowerInvariant();
        var normalizedCode = CollectionRules.NormalizeCode(code);
        return db.Collections.AnyAsync(x => !x.IsDeleted && x.SiteKey == normalizedSite && x.Code == normalizedCode &&
            (!excludeId.HasValue || x.Id != excludeId.Value), cancellationToken);
    }

    public async Task<IReadOnlyCollection<ContentCollection>> GetAllAsync(string? siteKey = null, CancellationToken cancellationToken = default)
    {
        var query = db.Collections.AsNoTracking().Where(x => !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(siteKey))
        {
            var normalizedSite = siteKey.Trim().ToLowerInvariant();
            query = query.Where(x => x.SiteKey == normalizedSite);
        }
        return await query.OrderBy(x => x.SiteKey).ThenBy(x => x.Code).ToListAsync(cancellationToken);
    }
}
