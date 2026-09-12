using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Content.Domain.Collections.Entities;

namespace Dhole.Content.Application.Abstractions.Repositories;

public interface ICollectionItemRepository : IRepository<ContentCollectionItem, Guid>
{
    Task<IReadOnlyCollection<ContentCollectionItem>> GetByCollectionAsync(Guid collectionId, CancellationToken cancellationToken = default);
}
