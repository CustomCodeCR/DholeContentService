using CustomCodeFramework.Postgres.EntityFramework.Repositories;
using Dhole.Content.Application.Abstractions.Repositories;
using Dhole.Content.Domain.Routes.Entities;
using Dhole.Content.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Dhole.Content.Persistence.Repositories;

public sealed class ContentRouteRepository(ServiceDbContext db)
    : EfRepository<ContentRoute, Guid>(db), IContentRouteRepository
{
    public Task<ContentRoute?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => db.ContentRoutes.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<ContentRoute?> GetByPathAsync(
        string siteKey,
        string locale,
        string path,
        bool activeOnly = true,
        CancellationToken ct = default)
    {
        var site = siteKey.Trim().ToLowerInvariant();
        var culture = locale.Trim();
        var normalizedPath = ContentRoute.NormalizePath(path);
        var query = db.ContentRoutes.AsNoTracking()
            .Where(x => x.SiteKey == site && x.Locale == culture && x.Path == normalizedPath);
        if (activeOnly) query = query.Where(x => x.IsActive);
        return query.FirstOrDefaultAsync(ct);
    }

    public Task<ContentRoute?> GetPrimaryAsync(Guid contentId, string locale, CancellationToken ct = default)
    {
        var culture = locale.Trim();
        return db.ContentRoutes.FirstOrDefaultAsync(
            x => x.ContentId == contentId && x.Locale == culture && x.IsPrimary,
            ct);
    }

    public async Task<IReadOnlyCollection<ContentRoute>> GetByContentAsync(Guid contentId, CancellationToken ct = default)
        => await db.ContentRoutes.AsNoTracking()
            .Where(x => x.ContentId == contentId)
            .OrderByDescending(x => x.IsPrimary)
            .ThenBy(x => x.Locale)
            .ThenBy(x => x.Path)
            .ToListAsync(ct);

    public Task<bool> ExistsPathAsync(
        string siteKey,
        string locale,
        string path,
        Guid? excludeId = null,
        CancellationToken ct = default)
    {
        var site = siteKey.Trim().ToLowerInvariant();
        var culture = locale.Trim();
        var normalizedPath = ContentRoute.NormalizePath(path);
        return db.ContentRoutes.AnyAsync(
            x => x.SiteKey == site && x.Locale == culture && x.Path == normalizedPath &&
                 (!excludeId.HasValue || x.Id != excludeId.Value),
            ct);
    }
}
