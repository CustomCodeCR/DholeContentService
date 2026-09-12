namespace Dhole.Content.Contracts.Campaigns;

public sealed record MarketingCampaignDto(
    Guid Id,
    string SiteKey,
    string Name,
    string Slug,
    string Status,
    DateTime? StartsAtUtc,
    DateTime? EndsAtUtc,
    Guid? LandingContentId,
    string? UtmSource,
    string? UtmMedium,
    string? UtmCampaign,
    string GoalType,
    string? SettingsJson,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);

public sealed record CreateMarketingCampaignRequest(
    string SiteKey,
    string Name,
    string Slug,
    string Status,
    DateTime? StartsAtUtc,
    DateTime? EndsAtUtc,
    Guid? LandingContentId,
    string? UtmSource,
    string? UtmMedium,
    string? UtmCampaign,
    string GoalType,
    string? SettingsJson);

public sealed record UpdateMarketingCampaignRequest(
    string SiteKey,
    string Name,
    string Slug,
    string Status,
    DateTime? StartsAtUtc,
    DateTime? EndsAtUtc,
    Guid? LandingContentId,
    string? UtmSource,
    string? UtmMedium,
    string? UtmCampaign,
    string GoalType,
    string? SettingsJson);
