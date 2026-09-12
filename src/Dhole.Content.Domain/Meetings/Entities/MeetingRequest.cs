using CustomCodeFramework.Core.Domain.Entities;

namespace Dhole.Content.Domain.Meetings.Entities;

public sealed class MeetingRequest : SoftDeletableAggregateRoot<Guid>
{
    private MeetingRequest() { }

    private MeetingRequest(Guid id, Guid meetingTypeId, Guid? leadId, Guid? submissionId,
        DateTime requestedStartUtc, DateTime requestedEndUtc, string timeZone, string subject,
        string? message, Guid? assignedUserId, Guid? actorUserId) : base(id)
    {
        if (meetingTypeId == Guid.Empty) throw new ArgumentException("MeetingTypeId is required.", nameof(meetingTypeId));
        if (!leadId.HasValue && !submissionId.HasValue)
            throw new ArgumentException("LeadId or SubmissionId is required.");
        MeetingRules.ValidateWindow(requestedStartUtc, requestedEndUtc);

        MeetingTypeId = meetingTypeId;
        LeadId = leadId;
        SubmissionId = submissionId;
        RequestedStartUtc = requestedStartUtc;
        RequestedEndUtc = requestedEndUtc;
        TimeZone = MeetingRules.NormalizeRequired(timeZone, 120, nameof(timeZone));
        Subject = MeetingRules.NormalizeRequired(subject, 240, nameof(subject));
        Message = MeetingRules.NormalizeOptional(message, 4000);
        Status = MeetingRules.StatusRequested;
        AssignedUserId = assignedUserId;
        MarkAsCreated(DateTime.UtcNow, actorUserId?.ToString());
    }

    public Guid MeetingTypeId { get; private set; }
    public Guid? LeadId { get; private set; }
    public Guid? SubmissionId { get; private set; }
    public DateTime RequestedStartUtc { get; private set; }
    public DateTime RequestedEndUtc { get; private set; }
    public string TimeZone { get; private set; } = "UTC";
    public string Subject { get; private set; } = string.Empty;
    public string? Message { get; private set; }
    public string Status { get; private set; } = MeetingRules.StatusRequested;
    public Guid? AssignedUserId { get; private set; }
    public DateTime? ConfirmedStartUtc { get; private set; }
    public DateTime? ConfirmedEndUtc { get; private set; }
    public string? ExternalProvider { get; private set; }
    public string? ExternalEventId { get; private set; }
    public string? MeetingUrl { get; private set; }

    public static MeetingRequest Create(Guid meetingTypeId, Guid? leadId, Guid? submissionId,
        DateTime requestedStartUtc, DateTime requestedEndUtc, string timeZone, string subject,
        string? message, Guid? assignedUserId, Guid? actorUserId)
        => new(Guid.NewGuid(), meetingTypeId, leadId, submissionId, requestedStartUtc, requestedEndUtc,
            timeZone, subject, message, assignedUserId, actorUserId);

    public void MarkPendingConfirmation(Guid? assignedUserId, Guid? actorUserId)
    {
        EnsureStatus(MeetingRules.StatusRequested);
        AssignedUserId = assignedUserId ?? AssignedUserId;
        Status = MeetingRules.StatusPendingConfirmation;
        MarkAsUpdated(DateTime.UtcNow, actorUserId?.ToString());
    }

    public void Confirm(DateTime confirmedStartUtc, DateTime confirmedEndUtc, Guid? assignedUserId,
        string? externalProvider, string? externalEventId, string? meetingUrl, Guid? actorUserId)
    {
        if (Status is not MeetingRules.StatusRequested and not MeetingRules.StatusPendingConfirmation)
            throw new InvalidOperationException("Only requested or pending meetings can be confirmed.");
        MeetingRules.ValidateWindow(confirmedStartUtc, confirmedEndUtc);
        ConfirmedStartUtc = confirmedStartUtc;
        ConfirmedEndUtc = confirmedEndUtc;
        AssignedUserId = assignedUserId ?? AssignedUserId;
        ExternalProvider = MeetingRules.NormalizeOptional(externalProvider, 120);
        ExternalEventId = MeetingRules.NormalizeOptional(externalEventId, 240);
        MeetingUrl = MeetingRules.NormalizeOptional(meetingUrl, 2048);
        Status = MeetingRules.StatusConfirmed;
        MarkAsUpdated(DateTime.UtcNow, actorUserId?.ToString());
    }

    public void Reject(Guid? actorUserId)
    {
        if (Status is not MeetingRules.StatusRequested and not MeetingRules.StatusPendingConfirmation)
            throw new InvalidOperationException("Only requested or pending meetings can be rejected.");
        Status = MeetingRules.StatusRejected;
        MarkAsUpdated(DateTime.UtcNow, actorUserId?.ToString());
    }

    public void Cancel(Guid? actorUserId)
    {
        if (Status is not MeetingRules.StatusRequested and not MeetingRules.StatusPendingConfirmation and not MeetingRules.StatusConfirmed)
            throw new InvalidOperationException("Meeting cannot be cancelled from its current status.");
        Status = MeetingRules.StatusCancelled;
        MarkAsUpdated(DateTime.UtcNow, actorUserId?.ToString());
    }

    public void Complete(Guid? actorUserId)
    {
        EnsureStatus(MeetingRules.StatusConfirmed);
        Status = MeetingRules.StatusCompleted;
        MarkAsUpdated(DateTime.UtcNow, actorUserId?.ToString());
    }

    private void EnsureStatus(string expected)
    {
        if (!string.Equals(Status, expected, StringComparison.Ordinal))
            throw new InvalidOperationException($"Meeting must be in {expected} status.");
    }
}
