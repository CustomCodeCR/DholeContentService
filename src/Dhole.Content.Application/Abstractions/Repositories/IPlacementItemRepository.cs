using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Content.Domain.Placements.Entities;

namespace Dhole.Content.Application.Abstractions.Repositories;

public interface IPlacementItemRepository : IRepository<PlacementItem, Guid>
{
    Task<bool> ExistsAsync(Guid placementId, Guid contentId, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<PlacementItem>> GetByPlacementAsync(Guid placementId, CancellationToken cancellationToken = default);
}
