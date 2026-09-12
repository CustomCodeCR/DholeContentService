using CustomCodeFramework.Postgres.EntityFramework.Repositories;
using Dhole.Content.Application.Abstractions.Repositories;
using Dhole.Content.Domain.Submissions.Entities;
using Dhole.Content.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Dhole.Content.Persistence.Repositories;

public sealed class MarketingSubmissionRepository(ServiceDbContext db)
    : EfRepository<MarketingSubmission, Guid>(db), IMarketingSubmissionRepository
{
    public async Task<IReadOnlyCollection<MarketingSubmission>> GetAllAsync(
        Guid? formId = null,
        string? status = null,
        DateTime? submittedFromUtc = null,
        DateTime? submittedToUtc = null,
        CancellationToken cancellationToken = default)
    {
        var query = db.MarketingSubmissions.AsNoTracking().Where(x => !x.IsDeleted);

        if (formId.HasValue) query = query.Where(x => x.FormId == formId.Value);
        if (!string.IsNullOrWhiteSpace(status))
        {
            var normalizedStatus = status.Trim();
            query = query.Where(x => x.Status == normalizedStatus);
        }
        if (submittedFromUtc.HasValue) query = query.Where(x => x.SubmittedAtUtc >= submittedFromUtc.Value);
        if (submittedToUtc.HasValue) query = query.Where(x => x.SubmittedAtUtc <= submittedToUtc.Value);

        return await query
            .OrderByDescending(x => x.SubmittedAtUtc)
            .ThenByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }
}
