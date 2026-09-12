using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Content.Domain.Forms.Entities;

namespace Dhole.Content.Application.Abstractions.Repositories;

public interface IMarketingFormFieldRepository : IRepository<MarketingFormField, Guid>
{
    Task<bool> ExistsByFieldKeyAsync(Guid formId, string fieldKey, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<MarketingFormField>> GetByFormAsync(Guid formId, CancellationToken cancellationToken = default);
}
