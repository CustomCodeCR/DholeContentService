using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Content.Domain.Submissions.Entities;

namespace Dhole.Content.Application.Abstractions.Repositories;

public interface IMarketingSubmissionRepository : IRepository<MarketingSubmission, Guid>
{
    Task<IReadOnlyCollection<MarketingSubmission>> GetAllAsync(
        Guid? formId = null,
        string? status = null,
        DateTime? submittedFromUtc = null,
        DateTime? submittedToUtc = null,
        CancellationToken cancellationToken = default);
}
