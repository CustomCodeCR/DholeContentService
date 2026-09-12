using CustomCodeFramework.Cqrs.Queries;
using Dhole.Content.Application.Abstractions.Repositories;
using Dhole.Content.Application.Mappings;
using Dhole.Content.Contracts.Sites;

namespace Dhole.Content.Application.Sites.GetSites;

public sealed class GetSitesQueryHandler(ISiteRepository sites)
    : IQueryHandler<GetSitesQuery, IReadOnlyCollection<SiteDto>>
{
    public async Task<IReadOnlyCollection<SiteDto>> HandleAsync(
        GetSitesQuery query,
        CancellationToken cancellationToken = default
    ) => (await sites.GetAllActiveRecordsAsync(cancellationToken))
        .Select(SiteMappings.ToDto)
        .ToArray();
}
