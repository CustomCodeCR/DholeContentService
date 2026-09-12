using CustomCodeFramework.Core.Domain.Entities;

namespace Dhole.Content.Domain.Submissions.Entities;

public sealed class MarketingSubmission : SoftDeletableAggregateRoot<Guid>
{
    private MarketingSubmission() { }

    private MarketingSubmission(
        Guid id,
        Guid formId,
        Guid? contentId,
        Guid? campaignId,
        DateTime submittedAtUtc,
        string sourceUrl,
        string? referrerUrl,
        string? utmSource,
        string? utmMedium,
        string? utmCampaign,
        string? utmContent,
        string? utmTerm,
        string payloadJson,
        string? ipHash,
        string? userAgent,
        Guid correlationId) : base(id)
    {
        if (formId == Guid.Empty) throw new ArgumentException("FormId is required.", nameof(formId));
        if (correlationId == Guid.Empty) throw new ArgumentException("CorrelationId is required.", nameof(correlationId));

        FormId = formId;
        ContentId = contentId;
        CampaignId = campaignId;
        SubmittedAtUtc = submittedAtUtc;
        Status = SubmissionRules.StatusReceived;
        SourceUrl = SubmissionRules.NormalizeOptionalText(sourceUrl, 2048)
            ?? throw new ArgumentException("SourceUrl is required.", nameof(sourceUrl));
        ReferrerUrl = SubmissionRules.NormalizeOptionalText(referrerUrl, 2048);
        UtmSource = SubmissionRules.NormalizeOptionalText(utmSource, 240);
        UtmMedium = SubmissionRules.NormalizeOptionalText(utmMedium, 240);
        UtmCampaign = SubmissionRules.NormalizeOptionalText(utmCampaign, 240);
        UtmContent = SubmissionRules.NormalizeOptionalText(utmContent, 240);
        UtmTerm = SubmissionRules.NormalizeOptionalText(utmTerm, 240);
        PayloadJson = SubmissionRules.NormalizePayloadJson(payloadJson);
        IpHash = SubmissionRules.NormalizeIpHash(ipHash);
        UserAgent = SubmissionRules.NormalizeOptionalText(userAgent, 1024);
        CorrelationId = correlationId;
        MarkAsCreated(submittedAtUtc, null);
    }

    public Guid FormId { get; private set; }
    public Guid? ContentId { get; private set; }
    public Guid? CampaignId { get; private set; }
    public DateTime SubmittedAtUtc { get; private set; }
    public string Status { get; private set; } = SubmissionRules.StatusReceived;
    public string SourceUrl { get; private set; } = string.Empty;
    public string? ReferrerUrl { get; private set; }
    public string? UtmSource { get; private set; }
    public string? UtmMedium { get; private set; }
    public string? UtmCampaign { get; private set; }
    public string? UtmContent { get; private set; }
    public string? UtmTerm { get; private set; }
    public string PayloadJson { get; private set; } = "{}";
    public string? IpHash { get; private set; }
    public string? UserAgent { get; private set; }
    public Guid CorrelationId { get; private set; }

    public static MarketingSubmission Create(
        Guid formId,
        Guid? contentId,
        Guid? campaignId,
        DateTime submittedAtUtc,
        string sourceUrl,
        string? referrerUrl,
        string? utmSource,
        string? utmMedium,
        string? utmCampaign,
        string? utmContent,
        string? utmTerm,
        string payloadJson,
        string? ipHash,
        string? userAgent,
        Guid correlationId)
        => new(Guid.NewGuid(), formId, contentId, campaignId, submittedAtUtc, sourceUrl, referrerUrl,
            utmSource, utmMedium, utmCampaign, utmContent, utmTerm, payloadJson, ipHash, userAgent, correlationId);
}
