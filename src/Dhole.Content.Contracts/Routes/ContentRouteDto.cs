namespace Dhole.Content.Contracts.Routes;

public sealed record ContentRouteDto(
    Guid Id,
    string SiteKey,
    Guid ContentId,
    string Locale,
    string Path,
    bool IsPrimary,
    bool IsActive,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc
);
