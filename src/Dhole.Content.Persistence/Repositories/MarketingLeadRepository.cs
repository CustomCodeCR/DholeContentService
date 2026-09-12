using CustomCodeFramework.Postgres.EntityFramework.Repositories;
using Dhole.Content.Application.Abstractions.Repositories;
using Dhole.Content.Domain.Leads;
using Dhole.Content.Domain.Leads.Entities;
using Dhole.Content.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Dhole.Content.Persistence.Repositories;

public sealed class MarketingLeadRepository(ServiceDbContext db)
    : EfRepository<MarketingLead, Guid>(db), IMarketingLeadRepository
{
    public Task<MarketingLead?> GetByEmailAsync(string siteKey, string email, CancellationToken cancellationToken = default)
    {
        var normalizedSite = MarketingLeadRules.NormalizeSiteKey(siteKey);
        var normalizedEmail = MarketingLeadRules.NormalizeEmail(email)!;
        return db.MarketingLeads.FirstOrDefaultAsync(x => !x.IsDeleted && x.SiteKey == normalizedSite && x.Email == normalizedEmail, cancellationToken);
    }

    public async Task<bool> ExistsByEmailAsync(string siteKey, string email, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var normalizedSite = MarketingLeadRules.NormalizeSiteKey(siteKey);
        var normalizedEmail = MarketingLeadRules.NormalizeEmail(email)!;
        return await db.MarketingLeads.AnyAsync(x => !x.IsDeleted && x.SiteKey == normalizedSite && x.Email == normalizedEmail &&
            (!excludeId.HasValue || x.Id != excludeId.Value), cancellationToken);
    }

    public async Task<IReadOnlyCollection<MarketingLead>> GetAllAsync(
        string? siteKey = null,
        string? status = null,
        Guid? ownerUserId = null,
        string? source = null,
        CancellationToken cancellationToken = default)
    {
        var query = db.MarketingLeads.AsNoTracking().Where(x => !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(siteKey))
        {
            var normalizedSite = siteKey.Trim().ToLowerInvariant();
            query = query.Where(x => x.SiteKey == normalizedSite);
        }
        if (!string.IsNullOrWhiteSpace(status))
        {
            var normalizedStatus = status.Trim();
            query = query.Where(x => x.Status == normalizedStatus);
        }
        if (ownerUserId.HasValue) query = query.Where(x => x.OwnerUserId == ownerUserId.Value);
        if (!string.IsNullOrWhiteSpace(source))
        {
            var normalizedSource = source.Trim();
            query = query.Where(x => x.Source == normalizedSource);
        }
        return await query.OrderByDescending(x => x.LastTouchAtUtc).ThenByDescending(x => x.CreatedAtUtc).ToListAsync(cancellationToken);
    }
}
