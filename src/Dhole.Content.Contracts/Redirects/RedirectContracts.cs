namespace Dhole.Content.Contracts.Redirects;

public sealed record ContentRedirectDto(
    Guid Id,
    string SiteKey,
    string SourcePath,
    string TargetUrl,
    int StatusCode,
    bool IsActive,
    DateTime? ValidFromUtc,
    DateTime? ValidToUtc,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc
);

public sealed record CreateRedirectRequest(
    string SiteKey,
    string SourcePath,
    string TargetUrl,
    int StatusCode,
    bool IsActive,
    DateTime? ValidFromUtc,
    DateTime? ValidToUtc
);

public sealed record UpdateRedirectRequest(
    string SiteKey,
    string SourcePath,
    string TargetUrl,
    int StatusCode,
    bool IsActive,
    DateTime? ValidFromUtc,
    DateTime? ValidToUtc
);

public sealed record RedirectResolutionDto(string TargetUrl, int StatusCode);
