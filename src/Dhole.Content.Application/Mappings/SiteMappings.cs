using Dhole.Content.Contracts.Sites;
using Dhole.Content.Domain.Sites.Entities;

namespace Dhole.Content.Application.Mappings;

public static class SiteMappings
{
    public static SiteDto ToDto(Site site) => new(
        site.Id,
        site.SiteKey,
        site.Name,
        site.PrimaryDomain,
        site.DefaultLocale,
        site.TimeZone,
        site.LogoMediaId,
        site.FaviconMediaId,
        site.DefaultOpenGraphMediaId,
        site.Status,
        site.CreatedAtUtc,
        site.UpdatedAtUtc
    );
}
