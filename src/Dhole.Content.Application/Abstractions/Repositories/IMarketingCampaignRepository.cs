using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Content.Domain.Campaigns.Entities;

namespace Dhole.Content.Application.Abstractions.Repositories;

public interface IMarketingCampaignRepository : IRepository<MarketingCampaign, Guid>
{
    Task<bool> ExistsBySlugAsync(string siteKey, string slug, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<MarketingCampaign>> GetAllAsync(string? siteKey = null, string? status = null, CancellationToken cancellationToken = default);
}
