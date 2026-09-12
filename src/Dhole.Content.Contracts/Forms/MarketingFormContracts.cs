namespace Dhole.Content.Contracts.Forms;

public sealed record MarketingFormDto(
    Guid Id,
    string SiteKey,
    string FormKey,
    string Name,
    string Purpose,
    string Status,
    string? SuccessMessage,
    string? NotificationTemplateKey,
    string? SettingsJson,
    int Version,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);

public sealed record MarketingFormFieldDto(
    Guid Id,
    Guid FormId,
    string FieldKey,
    string Label,
    string FieldType,
    string? Placeholder,
    bool IsRequired,
    int SortOrder,
    string? ValidationJson,
    string? OptionsJson,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);

public sealed record MarketingFormPurposeDto(string Value, string Label);

public sealed record CreateMarketingFormRequest(
    string SiteKey,
    string FormKey,
    string Name,
    string Purpose,
    string Status = "Draft",
    string? SuccessMessage = null,
    string? NotificationTemplateKey = null,
    string? SettingsJson = null);

public sealed record UpdateMarketingFormRequest(
    string SiteKey,
    string FormKey,
    string Name,
    string Purpose,
    string Status,
    string? SuccessMessage,
    string? NotificationTemplateKey,
    string? SettingsJson);

public sealed record CreateMarketingFormFieldRequest(
    string FieldKey,
    string Label,
    string FieldType,
    string? Placeholder,
    bool IsRequired,
    int SortOrder,
    string? ValidationJson,
    string? OptionsJson);

public sealed record UpdateMarketingFormFieldRequest(
    string FieldKey,
    string Label,
    string FieldType,
    string? Placeholder,
    bool IsRequired,
    int SortOrder,
    string? ValidationJson,
    string? OptionsJson);
