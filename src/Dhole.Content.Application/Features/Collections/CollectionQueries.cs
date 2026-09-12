using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Queries;
using Dhole.Content.Application.Abstractions.Repositories;
using Dhole.Content.Contracts.Collections;
using Dhole.Content.Domain.Collections.Entities;
using Dhole.Content.Domain.Shared;

namespace Dhole.Content.Application.Collections;

public sealed record GetCollectionsQuery(string? SiteKey) : IQuery<IReadOnlyCollection<CollectionDto>>;
public sealed record GetCollectionByIdQuery(Guid Id) : IQuery<Result<CollectionDto>>;
public sealed record GetCollectionItemsQuery(Guid CollectionId) : IQuery<Result<IReadOnlyCollection<CollectionItemDto>>>;

public sealed class GetCollectionsQueryHandler(ICollectionRepository collections)
    : IQueryHandler<GetCollectionsQuery, IReadOnlyCollection<CollectionDto>>
{
    public async Task<IReadOnlyCollection<CollectionDto>> HandleAsync(GetCollectionsQuery query, CancellationToken cancellationToken = default)
        => (await collections.GetAllAsync(query.SiteKey, cancellationToken)).Select(Map).ToArray();

    internal static CollectionDto Map(ContentCollection collection)
        => new(collection.Id, collection.SiteKey, collection.Code, collection.Name, collection.SettingsJson,
            collection.IsActive, collection.CreatedAtUtc, collection.UpdatedAtUtc);
}

public sealed class GetCollectionByIdQueryHandler(ICollectionRepository collections)
    : IQueryHandler<GetCollectionByIdQuery, Result<CollectionDto>>
{
    public async Task<Result<CollectionDto>> HandleAsync(GetCollectionByIdQuery query, CancellationToken cancellationToken = default)
    {
        var collection = await collections.GetByIdAsync(query.Id, cancellationToken);
        return collection is null || collection.IsDeleted
            ? Result.Failure<CollectionDto>(ContentErrors.CollectionNotFound)
            : Result.Success(GetCollectionsQueryHandler.Map(collection));
    }
}

public sealed class GetCollectionItemsQueryHandler(ICollectionRepository collections, ICollectionItemRepository items)
    : IQueryHandler<GetCollectionItemsQuery, Result<IReadOnlyCollection<CollectionItemDto>>>
{
    public async Task<Result<IReadOnlyCollection<CollectionItemDto>>> HandleAsync(GetCollectionItemsQuery query, CancellationToken cancellationToken = default)
    {
        var collection = await collections.GetByIdAsync(query.CollectionId, cancellationToken);
        if (collection is null || collection.IsDeleted)
            return Result.Failure<IReadOnlyCollection<CollectionItemDto>>(ContentErrors.CollectionNotFound);

        var result = (await items.GetByCollectionAsync(query.CollectionId, cancellationToken))
            .Select(item => new CollectionItemDto(item.Id, item.CollectionId, item.DataJson, item.SortOrder,
                item.IsActive, item.CreatedAtUtc, item.UpdatedAtUtc))
            .ToArray();
        return Result.Success<IReadOnlyCollection<CollectionItemDto>>(result);
    }
}
