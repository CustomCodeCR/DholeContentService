using CustomCodeFramework.Core.Domain.Entities;

namespace Dhole.Content.Domain.Campaigns.Entities;

public sealed class MarketingCampaign : SoftDeletableAggregateRoot<Guid>
{
    private MarketingCampaign() { }

    private MarketingCampaign(Guid id, string siteKey, string name, string slug, string status,
        DateTime? startsAtUtc, DateTime? endsAtUtc, Guid? landingContentId, string? utmSource,
        string? utmMedium, string? utmCampaign, string goalType, string? settingsJson, Guid? actorUserId) : base(id)
    {
        Apply(siteKey, name, slug, status, startsAtUtc, endsAtUtc, landingContentId,
            utmSource, utmMedium, utmCampaign, goalType, settingsJson);
        MarkAsCreated(DateTime.UtcNow, actorUserId?.ToString());
    }

    public string SiteKey { get; private set; } = "main";
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string Status { get; private set; } = string.Empty;
    public DateTime? StartsAtUtc { get; private set; }
    public DateTime? EndsAtUtc { get; private set; }
    public Guid? LandingContentId { get; private set; }
    public string? UtmSource { get; private set; }
    public string? UtmMedium { get; private set; }
    public string? UtmCampaign { get; private set; }
    public string GoalType { get; private set; } = string.Empty;
    public string? SettingsJson { get; private set; }

    public static MarketingCampaign Create(string siteKey, string name, string slug, string status,
        DateTime? startsAtUtc, DateTime? endsAtUtc, Guid? landingContentId, string? utmSource,
        string? utmMedium, string? utmCampaign, string goalType, string? settingsJson, Guid? actorUserId)
        => new(Guid.NewGuid(), siteKey, name, slug, status, startsAtUtc, endsAtUtc, landingContentId,
            utmSource, utmMedium, utmCampaign, goalType, settingsJson, actorUserId);

    public void Update(string siteKey, string name, string slug, string status,
        DateTime? startsAtUtc, DateTime? endsAtUtc, Guid? landingContentId, string? utmSource,
        string? utmMedium, string? utmCampaign, string goalType, string? settingsJson, Guid? actorUserId)
    {
        Apply(siteKey, name, slug, status, startsAtUtc, endsAtUtc, landingContentId,
            utmSource, utmMedium, utmCampaign, goalType, settingsJson);
        MarkAsUpdated(DateTime.UtcNow, actorUserId?.ToString());
    }

    public void Delete(Guid? actorUserId)
        => MarkAsDeleted(DateTime.UtcNow, actorUserId?.ToString());

    private void Apply(string siteKey, string name, string slug, string status,
        DateTime? startsAtUtc, DateTime? endsAtUtc, Guid? landingContentId, string? utmSource,
        string? utmMedium, string? utmCampaign, string goalType, string? settingsJson)
    {
        CampaignRules.ValidateWindow(startsAtUtc, endsAtUtc);
        SiteKey = CampaignRules.NormalizeSiteKey(siteKey);
        Name = CampaignRules.NormalizeName(name);
        Slug = CampaignRules.NormalizeSlug(slug);
        Status = CampaignRules.NormalizeStatus(status);
        StartsAtUtc = startsAtUtc?.ToUniversalTime();
        EndsAtUtc = endsAtUtc?.ToUniversalTime();
        LandingContentId = landingContentId;
        UtmSource = CampaignRules.NormalizeUtm(utmSource);
        UtmMedium = CampaignRules.NormalizeUtm(utmMedium);
        UtmCampaign = CampaignRules.NormalizeUtm(utmCampaign);
        GoalType = CampaignRules.NormalizeGoalType(goalType);
        SettingsJson = CampaignRules.NormalizeSettingsJson(settingsJson);
    }
}
