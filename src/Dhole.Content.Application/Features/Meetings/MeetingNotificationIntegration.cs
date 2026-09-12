using System.Text.Json;
using Dhole.Content.Domain.Forms.Entities;

namespace Dhole.Content.Application.Meetings;

public static class MeetingNotificationIntegration
{
    public const string RequestedEventName = "content.meeting.requested";
    public const string RequestedEventType = "Content.MeetingRequested";
    public const string ConfirmedEventName = "content.meeting.confirmed";
    public const string ConfirmedEventType = "Content.MeetingConfirmed";

    public static string? ResolveSubmissionEmail(
        string payloadJson,
        IReadOnlyCollection<MarketingFormField> fields)
    {
        var emailFields = fields
            .Where(field => !field.IsDeleted && string.Equals(field.FieldType, "email", StringComparison.OrdinalIgnoreCase))
            .OrderBy(field => field.SortOrder)
            .ToArray();
        if (emailFields.Length == 0 || string.IsNullOrWhiteSpace(payloadJson)) return null;

        try
        {
            using var document = JsonDocument.Parse(payloadJson);
            if (document.RootElement.ValueKind != JsonValueKind.Object) return null;
            foreach (var field in emailFields)
            {
                if (!document.RootElement.TryGetProperty(field.FieldKey, out var value) || value.ValueKind != JsonValueKind.String)
                    continue;
                var candidate = value.GetString()?.Trim().ToLowerInvariant();
                if (!string.IsNullOrWhiteSpace(candidate) && candidate.Contains('@')) return candidate;
            }
        }
        catch (JsonException)
        {
            return null;
        }

        return null;
    }
}

public sealed record MeetingRequestedNotificationEvent(
    Guid MeetingRequestId,
    Guid MeetingTypeId,
    string SiteKey,
    string MeetingTypeName,
    string MeetingMode,
    Guid? LeadId,
    Guid? SubmissionId,
    DateTime RequestedStartUtc,
    DateTime RequestedEndUtc,
    string TimeZone,
    string Subject,
    Guid? AssignedUserId,
    string? AssignedTeamKey);

public sealed record MeetingConfirmedNotificationEvent(
    Guid MeetingRequestId,
    Guid MeetingTypeId,
    string SiteKey,
    string MeetingTypeName,
    string MeetingMode,
    Guid? LeadId,
    Guid? SubmissionId,
    string ClientEmail,
    string? ClientName,
    DateTime ConfirmedStartUtc,
    DateTime ConfirmedEndUtc,
    string TimeZone,
    string Subject,
    string? MeetingUrl,
    string? ExternalProvider,
    string? ExternalEventId);
