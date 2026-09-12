using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Content.Domain.Leads.Entities;

namespace Dhole.Content.Application.Abstractions.Repositories;

public interface IMarketingLeadRepository : IRepository<MarketingLead, Guid>
{
    Task<MarketingLead?> GetByEmailAsync(string siteKey, string email, CancellationToken cancellationToken = default);
    Task<bool> ExistsByEmailAsync(string siteKey, string email, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<MarketingLead>> GetAllAsync(
        string? siteKey = null,
        string? status = null,
        Guid? ownerUserId = null,
        string? source = null,
        CancellationToken cancellationToken = default);
}
