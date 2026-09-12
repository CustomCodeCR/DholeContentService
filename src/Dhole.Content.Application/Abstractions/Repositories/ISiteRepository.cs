using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Content.Domain.Sites.Entities;

namespace Dhole.Content.Application.Abstractions.Repositories;

public interface ISiteRepository : IRepository<Site, Guid>
{
    Task<Site?> GetBySiteKeyAsync(string siteKey, CancellationToken cancellationToken = default);
    Task<bool> ExistsBySiteKeyAsync(string siteKey, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<bool> ExistsByPrimaryDomainAsync(string primaryDomain, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Site>> GetAllActiveRecordsAsync(CancellationToken cancellationToken = default);
}
