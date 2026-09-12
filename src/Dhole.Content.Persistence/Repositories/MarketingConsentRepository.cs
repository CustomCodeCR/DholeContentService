using CustomCodeFramework.Postgres.EntityFramework.Repositories;
using Dhole.Content.Application.Abstractions.Repositories;
using Dhole.Content.Domain.Consents.Entities;
using Dhole.Content.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Dhole.Content.Persistence.Repositories;

public sealed class MarketingConsentRepository(ServiceDbContext db)
    : EfRepository<MarketingConsent, Guid>(db), IMarketingConsentRepository
{
    public async Task<IReadOnlyCollection<MarketingConsent>> GetAllAsync(
        Guid? leadId = null,
        Guid? submissionId = null,
        string? purpose = null,
        bool? granted = null,
        DateTime? capturedFromUtc = null,
        DateTime? capturedToUtc = null,
        CancellationToken cancellationToken = default)
    {
        var query = db.MarketingConsents.AsNoTracking().Where(x => !x.IsDeleted);
        if (leadId.HasValue) query = query.Where(x => x.LeadId == leadId.Value);
        if (submissionId.HasValue) query = query.Where(x => x.SubmissionId == submissionId.Value);
        if (!string.IsNullOrWhiteSpace(purpose))
        {
            var normalizedPurpose = purpose.Trim().ToLowerInvariant().Replace('_', '-').Replace(' ', '-');
            query = query.Where(x => x.Purpose == normalizedPurpose);
        }
        if (granted.HasValue) query = query.Where(x => x.Granted == granted.Value);
        if (capturedFromUtc.HasValue) query = query.Where(x => x.CapturedAtUtc >= capturedFromUtc.Value);
        if (capturedToUtc.HasValue) query = query.Where(x => x.CapturedAtUtc <= capturedToUtc.Value);

        return await query
            .OrderByDescending(x => x.CapturedAtUtc)
            .ThenByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }
}
