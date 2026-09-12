using CustomCodeFramework.Core.Domain.Entities;

namespace Dhole.Content.Domain.Consents.Entities;

public sealed class MarketingConsent : SoftDeletableAggregateRoot<Guid>
{
    private MarketingConsent() { }

    private MarketingConsent(
        Guid id,
        Guid? leadId,
        Guid? submissionId,
        string purpose,
        bool granted,
        string policyVersion,
        string source,
        DateTime capturedAtUtc,
        Guid? actorUserId,
        DateTime utcNow) : base(id)
    {
        if (!leadId.HasValue && !submissionId.HasValue)
            throw new ArgumentException("A consent requires LeadId or SubmissionId.");

        LeadId = leadId;
        SubmissionId = submissionId;
        Purpose = MarketingConsentRules.NormalizePurpose(purpose);
        Granted = granted;
        PolicyVersion = MarketingConsentRules.NormalizePolicyVersion(policyVersion);
        Source = MarketingConsentRules.NormalizeSource(source);
        CapturedAtUtc = MarketingConsentRules.NormalizeCapturedAtUtc(capturedAtUtc, utcNow);
        MarkAsCreated(utcNow, actorUserId?.ToString());
    }

    public Guid? LeadId { get; private set; }
    public Guid? SubmissionId { get; private set; }
    public string Purpose { get; private set; } = string.Empty;
    public bool Granted { get; private set; }
    public string PolicyVersion { get; private set; } = string.Empty;
    public string Source { get; private set; } = string.Empty;
    public DateTime CapturedAtUtc { get; private set; }

    public static MarketingConsent Create(
        Guid? leadId,
        Guid? submissionId,
        string purpose,
        bool granted,
        string policyVersion,
        string source,
        DateTime? capturedAtUtc,
        Guid? actorUserId,
        DateTime utcNow)
        => new(Guid.NewGuid(), leadId, submissionId, purpose, granted, policyVersion, source,
            MarketingConsentRules.NormalizeCapturedAtUtc(capturedAtUtc, utcNow), actorUserId, utcNow);
}
