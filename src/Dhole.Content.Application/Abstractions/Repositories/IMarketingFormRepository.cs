using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Content.Domain.Forms.Entities;

namespace Dhole.Content.Application.Abstractions.Repositories;

public interface IMarketingFormRepository : IRepository<MarketingForm, Guid>
{
    Task<bool> ExistsByFormKeyAsync(string siteKey, string formKey, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<MarketingForm>> GetAllAsync(
        string? siteKey = null,
        string? purpose = null,
        string? status = null,
        CancellationToken cancellationToken = default);
}
