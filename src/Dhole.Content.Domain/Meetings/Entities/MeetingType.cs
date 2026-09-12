using CustomCodeFramework.Core.Domain.Entities;

namespace Dhole.Content.Domain.Meetings.Entities;

public sealed class MeetingType : SoftDeletableAggregateRoot<Guid>
{
    private MeetingType() { }

    private MeetingType(Guid id, string siteKey, string name, string slug, string? description,
        int durationMinutes, int bufferMinutes, string meetingMode, Guid? assignedUserId,
        string? assignedTeamKey, string? settingsJson, bool isActive, Guid? actorUserId) : base(id)
    {
        Apply(siteKey, name, slug, description, durationMinutes, bufferMinutes, meetingMode,
            assignedUserId, assignedTeamKey, settingsJson, isActive);
        MarkAsCreated(DateTime.UtcNow, actorUserId?.ToString());
    }

    public string SiteKey { get; private set; } = "main";
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public int DurationMinutes { get; private set; }
    public int BufferMinutes { get; private set; }
    public string MeetingMode { get; private set; } = string.Empty;
    public Guid? AssignedUserId { get; private set; }
    public string? AssignedTeamKey { get; private set; }
    public string? SettingsJson { get; private set; }
    public bool IsActive { get; private set; }

    public static MeetingType Create(string siteKey, string name, string slug, string? description,
        int durationMinutes, int bufferMinutes, string meetingMode, Guid? assignedUserId,
        string? assignedTeamKey, string? settingsJson, bool isActive, Guid? actorUserId)
        => new(Guid.NewGuid(), siteKey, name, slug, description, durationMinutes, bufferMinutes,
            meetingMode, assignedUserId, assignedTeamKey, settingsJson, isActive, actorUserId);

    public void Update(string siteKey, string name, string slug, string? description,
        int durationMinutes, int bufferMinutes, string meetingMode, Guid? assignedUserId,
        string? assignedTeamKey, string? settingsJson, bool isActive, Guid? actorUserId)
    {
        Apply(siteKey, name, slug, description, durationMinutes, bufferMinutes, meetingMode,
            assignedUserId, assignedTeamKey, settingsJson, isActive);
        MarkAsUpdated(DateTime.UtcNow, actorUserId?.ToString());
    }

    public void Delete(Guid? actorUserId) => MarkAsDeleted(DateTime.UtcNow, actorUserId?.ToString());

    private void Apply(string siteKey, string name, string slug, string? description,
        int durationMinutes, int bufferMinutes, string meetingMode, Guid? assignedUserId,
        string? assignedTeamKey, string? settingsJson, bool isActive)
    {
        MeetingRules.ValidateDuration(durationMinutes, bufferMinutes);
        SiteKey = MeetingRules.NormalizeSiteKey(siteKey);
        Name = MeetingRules.NormalizeRequired(name, 240, nameof(name));
        Slug = MeetingRules.NormalizeSlug(slug);
        Description = MeetingRules.NormalizeOptional(description, 4000);
        DurationMinutes = durationMinutes;
        BufferMinutes = bufferMinutes;
        MeetingMode = MeetingRules.NormalizeRequired(meetingMode, 80, nameof(meetingMode));
        AssignedUserId = assignedUserId;
        AssignedTeamKey = MeetingRules.NormalizeOptional(assignedTeamKey, 160);
        SettingsJson = MeetingRules.NormalizeSettingsJson(settingsJson);
        IsActive = isActive;
    }
}
