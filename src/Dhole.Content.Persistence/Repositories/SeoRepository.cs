using Dhole.Content.Application.Abstractions.Repositories;
using Dhole.Content.Domain.ContentItems.Enums;
using Dhole.Content.Domain.Seo;
using Dhole.Content.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Dhole.Content.Persistence.Repositories;

public sealed class SeoRepository(ServiceDbContext db) : ISeoRepository
{
    public async Task<IReadOnlyCollection<SeoSitemapSource>> GetPublishedSitemapSourcesAsync(
        string siteKey,
        DateTime utcNow,
        CancellationToken cancellationToken = default)
    {
        var key = siteKey.Trim().ToLowerInvariant();
        var rows = await (
            from route in db.ContentRoutes.AsNoTracking()
            join content in db.ContentItems.AsNoTracking() on route.ContentId equals content.Id
            where !route.IsDeleted && route.IsActive && route.IsPrimary && route.SiteKey == key
                  && !content.IsDeleted && content.Status == ContentStatus.Published
                  && (content.UnpublishAtUtc == null || content.UnpublishAtUtc > utcNow)
            orderby route.Path
            select new SeoSitemapSource(
                route.Path,
                content.CanonicalUrl,
                content.Robots,
                content.UpdatedAtUtc ?? content.PublishedAtUtc ?? content.CreatedAtUtc,
                content.SitemapPriority,
                content.SitemapChangeFrequency)
        ).ToListAsync(cancellationToken);

        return rows.Where(row => SeoRules.IsIndexable(row.Robots)).ToArray();
    }
}
