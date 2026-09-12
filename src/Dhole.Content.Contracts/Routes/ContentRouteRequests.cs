namespace Dhole.Content.Contracts.Routes;

public sealed record CreateContentRouteRequest(
    string SiteKey,
    Guid ContentId,
    string Locale,
    string Path,
    bool IsPrimary,
    bool IsActive
);

public sealed record UpdateContentRouteRequest(
    string SiteKey,
    string Locale,
    string Path,
    bool IsPrimary,
    bool IsActive
);
