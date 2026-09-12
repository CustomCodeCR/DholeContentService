using CustomCodeFramework.Cqrs.Queries;
using Dhole.Content.Application.Abstractions.Repositories;
using Dhole.Content.Application.Mappings;
using Dhole.Content.Contracts.Routes;

namespace Dhole.Content.Application.Routes.GetContentRoutes;

public sealed class GetContentRoutesQueryHandler(IContentRouteRepository routes)
    : IQueryHandler<GetContentRoutesQuery, IReadOnlyCollection<ContentRouteDto>>
{
    public async Task<IReadOnlyCollection<ContentRouteDto>> HandleAsync(
        GetContentRoutesQuery query,
        CancellationToken ct = default)
        => (await routes.GetByContentAsync(query.ContentId, ct))
            .Select(ContentRouteMappings.ToDto)
            .ToArray();
}
