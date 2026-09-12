using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Queries;
using Dhole.Content.Application.Abstractions.Repositories;
using Dhole.Content.Contracts.Placements;
using Dhole.Content.Domain.Placements.Entities;
using Dhole.Content.Domain.Shared;

namespace Dhole.Content.Application.Placements;

public sealed record GetPlacementsQuery(string? SiteKey) : IQuery<IReadOnlyCollection<PlacementDto>>;
public sealed record GetPlacementByIdQuery(Guid Id) : IQuery<Result<PlacementDto>>;
public sealed record GetPlacementItemsQuery(Guid PlacementId) : IQuery<Result<IReadOnlyCollection<PlacementItemDto>>>;

public sealed class GetPlacementsQueryHandler(IPlacementRepository placements)
    : IQueryHandler<GetPlacementsQuery, IReadOnlyCollection<PlacementDto>>
{
    public async Task<IReadOnlyCollection<PlacementDto>> HandleAsync(GetPlacementsQuery query, CancellationToken cancellationToken = default)
        => (await placements.GetAllAsync(query.SiteKey, cancellationToken)).Select(Map).ToArray();

    internal static PlacementDto Map(Placement placement)
        => new(placement.Id, placement.SiteKey, placement.Code, placement.Name, placement.AllowedTypesJson,
            placement.MaxItems, placement.SettingsJson, placement.IsActive, placement.CreatedAtUtc, placement.UpdatedAtUtc);
}

public sealed class GetPlacementByIdQueryHandler(IPlacementRepository placements)
    : IQueryHandler<GetPlacementByIdQuery, Result<PlacementDto>>
{
    public async Task<Result<PlacementDto>> HandleAsync(GetPlacementByIdQuery query, CancellationToken cancellationToken = default)
    {
        var placement = await placements.GetByIdAsync(query.Id, cancellationToken);
        return placement is null || placement.IsDeleted
            ? Result.Failure<PlacementDto>(ContentErrors.PlacementNotFound)
            : Result.Success(GetPlacementsQueryHandler.Map(placement));
    }
}

public sealed class GetPlacementItemsQueryHandler(IPlacementRepository placements, IPlacementItemRepository items)
    : IQueryHandler<GetPlacementItemsQuery, Result<IReadOnlyCollection<PlacementItemDto>>>
{
    public async Task<Result<IReadOnlyCollection<PlacementItemDto>>> HandleAsync(GetPlacementItemsQuery query, CancellationToken cancellationToken = default)
    {
        var placement = await placements.GetByIdAsync(query.PlacementId, cancellationToken);
        if (placement is null || placement.IsDeleted)
            return Result.Failure<IReadOnlyCollection<PlacementItemDto>>(ContentErrors.PlacementNotFound);

        var result = (await items.GetByPlacementAsync(query.PlacementId, cancellationToken))
            .Select(item => new PlacementItemDto(item.Id, item.PlacementId, item.ContentId, item.SortOrder,
                item.ValidFromUtc, item.ValidToUtc, item.SettingsJson, item.IsActive, item.CreatedAtUtc, item.UpdatedAtUtc))
            .ToArray();
        return Result.Success<IReadOnlyCollection<PlacementItemDto>>(result);
    }
}
