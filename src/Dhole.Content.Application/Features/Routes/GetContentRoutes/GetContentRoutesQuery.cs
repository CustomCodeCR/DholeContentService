using CustomCodeFramework.Cqrs.Queries;
using Dhole.Content.Contracts.Routes;

namespace Dhole.Content.Application.Routes.GetContentRoutes;

public sealed record GetContentRoutesQuery(Guid ContentId)
    : IQuery<IReadOnlyCollection<ContentRouteDto>>;
