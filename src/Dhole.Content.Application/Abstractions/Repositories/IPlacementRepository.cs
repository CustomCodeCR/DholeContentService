using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Content.Domain.Placements.Entities;

namespace Dhole.Content.Application.Abstractions.Repositories;

public interface IPlacementRepository : IRepository<Placement, Guid>
{
    Task<bool> ExistsByCodeAsync(string siteKey, string code, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Placement>> GetAllAsync(string? siteKey = null, CancellationToken cancellationToken = default);
}
