using Dhole.Content.Contracts.Routes;
using Dhole.Content.Domain.Routes.Entities;

namespace Dhole.Content.Application.Mappings;

public static class ContentRouteMappings
{
    public static ContentRouteDto ToDto(ContentRoute route) => new(
        route.Id,
        route.SiteKey,
        route.ContentId,
        route.Locale,
        route.Path,
        route.IsPrimary,
        route.IsActive,
        route.CreatedAtUtc,
        route.UpdatedAtUtc
    );
}
