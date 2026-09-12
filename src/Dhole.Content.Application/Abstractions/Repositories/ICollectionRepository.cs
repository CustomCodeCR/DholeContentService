using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Content.Domain.Collections.Entities;

namespace Dhole.Content.Application.Abstractions.Repositories;

public interface ICollectionRepository : IRepository<ContentCollection, Guid>
{
    Task<bool> ExistsByCodeAsync(string siteKey, string code, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<ContentCollection>> GetAllAsync(string? siteKey = null, CancellationToken cancellationToken = default);
}
