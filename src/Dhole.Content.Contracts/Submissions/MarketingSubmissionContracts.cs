using System.Text.Json;

namespace Dhole.Content.Contracts.Submissions;

public sealed record MarketingSubmissionDto(
    Guid Id,
    Guid FormId,
    Guid? ContentId,
    Guid? CampaignId,
    DateTime SubmittedAtUtc,
    string Status,
    string SourceUrl,
    string? ReferrerUrl,
    string? UtmSource,
    string? UtmMedium,
    string? UtmCampaign,
    string? UtmContent,
    string? UtmTerm,
    string PayloadJson,
    string? IpHash,
    string? UserAgent,
    Guid CorrelationId);

public sealed record SubmitMarketingFormRequest(
    Guid? ContentId,
    Guid? CampaignId,
    string SourceUrl,
    string? ReferrerUrl,
    string? UtmSource,
    string? UtmMedium,
    string? UtmCampaign,
    string? UtmContent,
    string? UtmTerm,
    JsonElement Payload);

public sealed record MarketingSubmissionReceiptDto(
    Guid SubmissionId,
    DateTime SubmittedAtUtc,
    string? SuccessMessage);
