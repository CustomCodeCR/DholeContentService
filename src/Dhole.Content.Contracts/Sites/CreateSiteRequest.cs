namespace Dhole.Content.Contracts.Sites;

public sealed record CreateSiteRequest(
    string SiteKey,
    string Name,
    string PrimaryDomain,
    string DefaultLocale,
    string TimeZone,
    Guid? LogoMediaId,
    Guid? FaviconMediaId,
    Guid? DefaultOpenGraphMediaId,
    string? Status
);
