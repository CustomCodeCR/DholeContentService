using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Content.Domain.Consents.Entities;

namespace Dhole.Content.Application.Abstractions.Repositories;

public interface IMarketingConsentRepository : IRepository<MarketingConsent, Guid>
{
    Task<IReadOnlyCollection<MarketingConsent>> GetAllAsync(
        Guid? leadId = null,
        Guid? submissionId = null,
        string? purpose = null,
        bool? granted = null,
        DateTime? capturedFromUtc = null,
        DateTime? capturedToUtc = null,
        CancellationToken cancellationToken = default);
}
