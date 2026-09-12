using CustomCodeFramework.Postgres.EntityFramework.Repositories;
using Dhole.Content.Application.Abstractions.Repositories;
using Dhole.Content.Domain.Sites.Entities;
using Dhole.Content.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Dhole.Content.Persistence.Repositories;

public sealed class SiteRepository(ServiceDbContext db)
    : EfRepository<Site, Guid>(db), ISiteRepository
{
    public Task<Site?> GetBySiteKeyAsync(string siteKey, CancellationToken cancellationToken = default)
    {
        var key = siteKey.Trim().ToLowerInvariant();
        return db.Sites.FirstOrDefaultAsync(x => x.SiteKey == key && !x.IsDeleted, cancellationToken);
    }

    public Task<bool> ExistsBySiteKeyAsync(string siteKey, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var key = siteKey.Trim().ToLowerInvariant();
        return db.Sites.AnyAsync(
            x => x.SiteKey == key && !x.IsDeleted && (!excludeId.HasValue || x.Id != excludeId.Value),
            cancellationToken
        );
    }

    public Task<bool> ExistsByPrimaryDomainAsync(string primaryDomain, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var domain = NormalizeDomain(primaryDomain);
        return db.Sites.AnyAsync(
            x => x.PrimaryDomain == domain && !x.IsDeleted && (!excludeId.HasValue || x.Id != excludeId.Value),
            cancellationToken
        );
    }

    public async Task<IReadOnlyCollection<Site>> GetAllActiveRecordsAsync(CancellationToken cancellationToken = default)
        => await db.Sites.AsNoTracking()
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

    private static string NormalizeDomain(string value)
    {
        var domain = value.Trim().ToLowerInvariant();
        if (domain.StartsWith("https://", StringComparison.OrdinalIgnoreCase)) domain = domain[8..];
        else if (domain.StartsWith("http://", StringComparison.OrdinalIgnoreCase)) domain = domain[7..];
        return domain.TrimEnd('/');
    }
}
