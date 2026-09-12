namespace Dhole.Content.Contracts.Consents;

public sealed record MarketingConsentDto(
    Guid Id,
    Guid? LeadId,
    Guid? SubmissionId,
    string Purpose,
    bool Granted,
    string PolicyVersion,
    string Source,
    DateTime CapturedAtUtc,
    DateTime CreatedAtUtc);

public sealed record CaptureMarketingConsentRequest(
    Guid? LeadId,
    Guid? SubmissionId,
    string Purpose,
    bool Granted,
    string PolicyVersion,
    string Source,
    DateTime? CapturedAtUtc);

public sealed record SubmitMarketingConsentRequest(
    string Purpose,
    bool Granted,
    string PolicyVersion,
    string? Source = null);

public sealed record MarketingConsentPurposeDto(string Value, string Label);
