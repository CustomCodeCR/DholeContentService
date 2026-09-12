namespace Dhole.Content.Contracts.Meetings;

public sealed record MeetingTypeDto(Guid Id, string SiteKey, string Name, string Slug, string? Description,
    int DurationMinutes, int BufferMinutes, string MeetingMode, Guid? AssignedUserId,
    string? AssignedTeamKey, string? SettingsJson, bool IsActive, DateTime CreatedAtUtc, DateTime? UpdatedAtUtc);

public sealed record MeetingRequestDto(Guid Id, Guid MeetingTypeId, Guid? LeadId, Guid? SubmissionId,
    DateTime RequestedStartUtc, DateTime RequestedEndUtc, string TimeZone, string Subject, string? Message,
    string Status, Guid? AssignedUserId, DateTime? ConfirmedStartUtc, DateTime? ConfirmedEndUtc,
    string? ExternalProvider, string? ExternalEventId, string? MeetingUrl, DateTime CreatedAtUtc, DateTime? UpdatedAtUtc);

public sealed record CreateMeetingTypeRequest(string SiteKey, string Name, string Slug, string? Description,
    int DurationMinutes, int BufferMinutes, string MeetingMode, Guid? AssignedUserId,
    string? AssignedTeamKey, string? SettingsJson, bool IsActive = true);

public sealed record UpdateMeetingTypeRequest(string SiteKey, string Name, string Slug, string? Description,
    int DurationMinutes, int BufferMinutes, string MeetingMode, Guid? AssignedUserId,
    string? AssignedTeamKey, string? SettingsJson, bool IsActive);

public sealed record CreateMeetingRequest(Guid MeetingTypeId, Guid? LeadId, Guid? SubmissionId,
    DateTime RequestedStartUtc, DateTime? RequestedEndUtc, string TimeZone, string Subject, string? Message);

public sealed record PendingMeetingRequest(Guid? AssignedUserId);
public sealed record ConfirmMeetingRequest(DateTime ConfirmedStartUtc, DateTime ConfirmedEndUtc,
    Guid? AssignedUserId, string? ExternalProvider, string? ExternalEventId, string? MeetingUrl);
