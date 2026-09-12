using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Queries;
using Dhole.Content.Application.Abstractions.Repositories;
using Dhole.Content.Application.Mappings;
using Dhole.Content.Contracts.Sites;
using Dhole.Content.Domain.Shared;

namespace Dhole.Content.Application.Sites.GetSiteByKey;

public sealed class GetSiteByKeyQueryHandler(ISiteRepository sites)
    : IQueryHandler<GetSiteByKeyQuery, Result<SiteDto>>
{
    public async Task<Result<SiteDto>> HandleAsync(
        GetSiteByKeyQuery query,
        CancellationToken cancellationToken = default
    )
    {
        var site = await sites.GetBySiteKeyAsync(query.SiteKey, cancellationToken);
        return site is null
            ? Result.Failure<SiteDto>(ContentErrors.SiteNotFound)
            : Result.Success(SiteMappings.ToDto(site));
    }
}
