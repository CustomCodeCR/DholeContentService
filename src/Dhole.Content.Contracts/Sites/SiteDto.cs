namespace Dhole.Content.Contracts.Sites;

public sealed record SiteDto(
    Guid Id,
    string SiteKey,
    string Name,
    string PrimaryDomain,
    string DefaultLocale,
    string TimeZone,
    Guid? LogoMediaId,
    Guid? FaviconMediaId,
    Guid? DefaultOpenGraphMediaId,
    string Status,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc
);
