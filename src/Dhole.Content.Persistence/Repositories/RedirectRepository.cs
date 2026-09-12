using CustomCodeFramework.Postgres.EntityFramework.Repositories;
using Dhole.Content.Application.Abstractions.Repositories;
using Dhole.Content.Domain.Redirects.Entities;
using Dhole.Content.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Dhole.Content.Persistence.Repositories;

public sealed class RedirectRepository(ServiceDbContext db)
    : EfRepository<ContentRedirect, Guid>(db), IRedirectRepository
{
    public Task<ContentRedirect?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => db.Redirects.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

    public Task<ContentRedirect?> GetBySourcePathAsync(string siteKey, string sourcePath, CancellationToken cancellationToken = default)
    {
        var site = ContentRedirect.NormalizeSiteKey(siteKey);
        var path = ContentRedirect.NormalizeSourcePath(sourcePath);
        return db.Redirects.FirstOrDefaultAsync(x => !x.IsDeleted && x.SiteKey == site && x.SourcePath == path, cancellationToken);
    }

    public async Task<IReadOnlyCollection<ContentRedirect>> GetAllAsync(string? siteKey = null, CancellationToken cancellationToken = default)
    {
        var query = db.Redirects.AsNoTracking().Where(x => !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(siteKey))
        {
            var site = ContentRedirect.NormalizeSiteKey(siteKey);
            query = query.Where(x => x.SiteKey == site);
        }
        return await query.OrderBy(x => x.SourcePath).ToListAsync(cancellationToken);
    }

    public Task<bool> ExistsSourcePathAsync(string siteKey, string sourcePath, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var site = ContentRedirect.NormalizeSiteKey(siteKey);
        var path = ContentRedirect.NormalizeSourcePath(sourcePath);
        return db.Redirects.AnyAsync(x => !x.IsDeleted && x.SiteKey == site && x.SourcePath == path &&
            (!excludeId.HasValue || x.Id != excludeId.Value), cancellationToken);
    }

    public Task<ContentRedirect?> ResolveAsync(string siteKey, string sourcePath, DateTime utcNow, CancellationToken cancellationToken = default)
    {
        var site = ContentRedirect.NormalizeSiteKey(siteKey);
        var path = ContentRedirect.NormalizeSourcePath(sourcePath);
        return db.Redirects.AsNoTracking().FirstOrDefaultAsync(x => !x.IsDeleted && x.SiteKey == site &&
            x.SourcePath == path && x.IsActive &&
            (!x.ValidFromUtc.HasValue || x.ValidFromUtc <= utcNow) &&
            (!x.ValidToUtc.HasValue || x.ValidToUtc > utcNow), cancellationToken);
    }
}
