using CustomCodeFramework.Postgres.EntityFramework.Repositories;
using Dhole.Content.Application.Abstractions.Repositories;
using Dhole.Content.Domain.Campaigns;
using Dhole.Content.Domain.Campaigns.Entities;
using Dhole.Content.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Dhole.Content.Persistence.Repositories;

public sealed class MarketingCampaignRepository(ServiceDbContext db)
    : EfRepository<MarketingCampaign, Guid>(db), IMarketingCampaignRepository
{
    public Task<bool> ExistsBySlugAsync(string siteKey, string slug, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var normalizedSite = CampaignRules.NormalizeSiteKey(siteKey);
        var normalizedSlug = CampaignRules.NormalizeSlug(slug);
        return db.MarketingCampaigns.AnyAsync(x => !x.IsDeleted && x.SiteKey == normalizedSite && x.Slug == normalizedSlug &&
            (!excludeId.HasValue || x.Id != excludeId.Value), cancellationToken);
    }

    public async Task<IReadOnlyCollection<MarketingCampaign>> GetAllAsync(string? siteKey = null, string? status = null, CancellationToken cancellationToken = default)
    {
        var query = db.MarketingCampaigns.AsNoTracking().Where(x => !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(siteKey))
        {
            var normalizedSite = CampaignRules.NormalizeSiteKey(siteKey);
            query = query.Where(x => x.SiteKey == normalizedSite);
        }
        if (!string.IsNullOrWhiteSpace(status))
        {
            var normalizedStatus = CampaignRules.NormalizeStatus(status);
            query = query.Where(x => x.Status == normalizedStatus);
        }
        return await query.OrderByDescending(x => x.StartsAtUtc ?? x.CreatedAtUtc).ThenBy(x => x.Name).ToListAsync(cancellationToken);
    }
}
