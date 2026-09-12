using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Queries;
using Dhole.Content.Application.Abstractions.Repositories;
using Dhole.Content.Application.Mappings;
using Dhole.Content.Contracts.ContentItems;
using Dhole.Content.Domain.ContentItems.Enums;
using Dhole.Content.Domain.Shared;

namespace Dhole.Content.Application.Routes.GetPublishedContentByRoute;

public sealed class GetPublishedContentByRouteQueryHandler(
    IContentRouteRepository routes,
    IContentItemRepository contents
) : IQueryHandler<GetPublishedContentByRouteQuery, Result<ContentItemDto>>
{
    public async Task<Result<ContentItemDto>> HandleAsync(
        GetPublishedContentByRouteQuery query,
        CancellationToken ct = default)
    {
        var route = await routes.GetByPathAsync(query.SiteKey, query.Locale, query.Path, true, ct);
        if (route is null)
            return Result.Failure<ContentItemDto>(ContentErrors.ContentRouteNotFound);

        var content = await contents.GetByIdWithDetailsAsync(route.ContentId, ct);
        if (content is null || content.IsDeleted || content.Status != ContentStatus.Published)
            return Result.Failure<ContentItemDto>(ContentErrors.ContentNotFound);

        if (!string.Equals(content.SiteKey, route.SiteKey, StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(content.Locale, route.Locale, StringComparison.OrdinalIgnoreCase))
            return Result.Failure<ContentItemDto>(ContentErrors.ContentRouteSiteMismatch);

        return Result.Success(ContentMappings.ToDto(content));
    }
}
