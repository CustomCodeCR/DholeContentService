using CustomCodeFramework.Postgres.EntityFramework.Repositories;
using Dhole.Content.Application.Abstractions.Repositories;
using Dhole.Content.Domain.Collections.Entities;
using Dhole.Content.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Dhole.Content.Persistence.Repositories;

public sealed class CollectionItemRepository(ServiceDbContext db) : EfRepository<ContentCollectionItem, Guid>(db), ICollectionItemRepository
{
    public async Task<IReadOnlyCollection<ContentCollectionItem>> GetByCollectionAsync(Guid collectionId, CancellationToken cancellationToken = default)
        => await db.CollectionItems.AsNoTracking()
            .Where(x => x.CollectionId == collectionId && !x.IsDeleted)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);
}
