using CustomCodeFramework.Core.Domain.Entities;

namespace Dhole.Content.Domain.Forms.Entities;

public sealed class MarketingForm : SoftDeletableAggregateRoot<Guid>
{
    private MarketingForm() { }

    private MarketingForm(
        Guid id,
        string siteKey,
        string formKey,
        string name,
        string purpose,
        string status,
        string? successMessage,
        string? notificationTemplateKey,
        string? settingsJson,
        Guid? actorUserId) : base(id)
    {
        Version = 1;
        Apply(siteKey, formKey, name, purpose, status, successMessage, notificationTemplateKey, settingsJson);
        MarkAsCreated(DateTime.UtcNow, actorUserId?.ToString());
    }

    public string SiteKey { get; private set; } = "main";
    public string FormKey { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Purpose { get; private set; } = MarketingFormRules.PurposeContact;
    public string Status { get; private set; } = MarketingFormRules.StatusDraft;
    public string? SuccessMessage { get; private set; }
    public string? NotificationTemplateKey { get; private set; }
    public string? SettingsJson { get; private set; }
    public int Version { get; private set; }

    public static MarketingForm Create(
        string siteKey,
        string formKey,
        string name,
        string purpose,
        string status,
        string? successMessage,
        string? notificationTemplateKey,
        string? settingsJson,
        Guid? actorUserId)
        => new(Guid.NewGuid(), siteKey, formKey, name, purpose, status, successMessage,
            notificationTemplateKey, settingsJson, actorUserId);

    public void Update(
        string siteKey,
        string formKey,
        string name,
        string purpose,
        string status,
        string? successMessage,
        string? notificationTemplateKey,
        string? settingsJson,
        Guid? actorUserId)
    {
        Apply(siteKey, formKey, name, purpose, status, successMessage, notificationTemplateKey, settingsJson);
        Version++;
        MarkAsUpdated(DateTime.UtcNow, actorUserId?.ToString());
    }

    public void TouchFieldChange(Guid? actorUserId)
    {
        Version++;
        MarkAsUpdated(DateTime.UtcNow, actorUserId?.ToString());
    }

    public void Delete(Guid? actorUserId)
        => MarkAsDeleted(DateTime.UtcNow, actorUserId?.ToString());

    private void Apply(
        string siteKey,
        string formKey,
        string name,
        string purpose,
        string status,
        string? successMessage,
        string? notificationTemplateKey,
        string? settingsJson)
    {
        if (string.IsNullOrWhiteSpace(siteKey)) throw new ArgumentException("SiteKey is required.", nameof(siteKey));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required.", nameof(name));

        SiteKey = siteKey.Trim().ToLowerInvariant();
        FormKey = MarketingFormRules.NormalizeFormKey(formKey);
        Name = name.Trim();
        Purpose = MarketingFormRules.NormalizePurpose(purpose);
        Status = MarketingFormRules.NormalizeStatus(status);
        SuccessMessage = MarketingFormRules.NormalizeOptionalText(successMessage);
        NotificationTemplateKey = MarketingFormRules.NormalizeOptionalText(notificationTemplateKey);
        SettingsJson = MarketingFormRules.NormalizeObjectJson(settingsJson, nameof(settingsJson));
    }
}
