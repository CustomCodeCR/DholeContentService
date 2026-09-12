using CustomCodeFramework.Postgres.EntityFramework.Repositories;
using Dhole.Content.Application.Abstractions.Repositories;
using Dhole.Content.Domain.Forms;
using Dhole.Content.Domain.Forms.Entities;
using Dhole.Content.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Dhole.Content.Persistence.Repositories;

public sealed class MarketingFormRepository(ServiceDbContext db) : EfRepository<MarketingForm, Guid>(db), IMarketingFormRepository
{
    public Task<bool> ExistsByFormKeyAsync(string siteKey, string formKey, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var normalizedSite = siteKey.Trim().ToLowerInvariant();
        var normalizedKey = MarketingFormRules.NormalizeFormKey(formKey);
        return db.MarketingForms.AnyAsync(x => !x.IsDeleted && x.SiteKey == normalizedSite && x.FormKey == normalizedKey &&
            (!excludeId.HasValue || x.Id != excludeId.Value), cancellationToken);
    }

    public async Task<IReadOnlyCollection<MarketingForm>> GetAllAsync(
        string? siteKey = null,
        string? purpose = null,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = db.MarketingForms.AsNoTracking().Where(x => !x.IsDeleted);

        if (!string.IsNullOrWhiteSpace(siteKey))
        {
            var normalizedSite = siteKey.Trim().ToLowerInvariant();
            query = query.Where(x => x.SiteKey == normalizedSite);
        }

        if (!string.IsNullOrWhiteSpace(purpose))
        {
            var normalizedPurpose = MarketingFormRules.NormalizePurpose(purpose);
            query = query.Where(x => x.Purpose == normalizedPurpose);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            var normalizedStatus = MarketingFormRules.NormalizeStatus(status);
            query = query.Where(x => x.Status == normalizedStatus);
        }

        return await query.OrderBy(x => x.SiteKey).ThenBy(x => x.FormKey).ToListAsync(cancellationToken);
    }
}
