using CustomCodeFramework.Core.Domain.Entities;

namespace Dhole.Content.Domain.Forms.Entities;

public sealed class MarketingFormField : SoftDeletableAggregateRoot<Guid>
{
    private MarketingFormField() { }

    private MarketingFormField(
        Guid id,
        Guid formId,
        string fieldKey,
        string label,
        string fieldType,
        string? placeholder,
        bool isRequired,
        int sortOrder,
        string? validationJson,
        string? optionsJson,
        Guid? actorUserId) : base(id)
    {
        FormId = formId;
        Apply(fieldKey, label, fieldType, placeholder, isRequired, sortOrder, validationJson, optionsJson);
        MarkAsCreated(DateTime.UtcNow, actorUserId?.ToString());
    }

    public Guid FormId { get; private set; }
    public string FieldKey { get; private set; } = string.Empty;
    public string Label { get; private set; } = string.Empty;
    public string FieldType { get; private set; } = string.Empty;
    public string? Placeholder { get; private set; }
    public bool IsRequired { get; private set; }
    public int SortOrder { get; private set; }
    public string? ValidationJson { get; private set; }
    public string? OptionsJson { get; private set; }

    public static MarketingFormField Create(
        Guid formId,
        string fieldKey,
        string label,
        string fieldType,
        string? placeholder,
        bool isRequired,
        int sortOrder,
        string? validationJson,
        string? optionsJson,
        Guid? actorUserId)
    {
        if (formId == Guid.Empty) throw new ArgumentException("FormId is required.", nameof(formId));
        return new MarketingFormField(Guid.NewGuid(), formId, fieldKey, label, fieldType, placeholder,
            isRequired, sortOrder, validationJson, optionsJson, actorUserId);
    }

    public void Update(
        string fieldKey,
        string label,
        string fieldType,
        string? placeholder,
        bool isRequired,
        int sortOrder,
        string? validationJson,
        string? optionsJson,
        Guid? actorUserId)
    {
        Apply(fieldKey, label, fieldType, placeholder, isRequired, sortOrder, validationJson, optionsJson);
        MarkAsUpdated(DateTime.UtcNow, actorUserId?.ToString());
    }

    public void Delete(Guid? actorUserId)
        => MarkAsDeleted(DateTime.UtcNow, actorUserId?.ToString());

    private void Apply(
        string fieldKey,
        string label,
        string fieldType,
        string? placeholder,
        bool isRequired,
        int sortOrder,
        string? validationJson,
        string? optionsJson)
    {
        if (string.IsNullOrWhiteSpace(label)) throw new ArgumentException("Label is required.", nameof(label));
        if (sortOrder < 0) throw new ArgumentOutOfRangeException(nameof(sortOrder));

        FieldKey = MarketingFormRules.NormalizeFieldKey(fieldKey);
        Label = label.Trim();
        FieldType = MarketingFormRules.NormalizeFieldType(fieldType);
        Placeholder = MarketingFormRules.NormalizeOptionalText(placeholder);
        IsRequired = isRequired;
        SortOrder = sortOrder;
        ValidationJson = MarketingFormRules.NormalizeObjectJson(validationJson, nameof(validationJson));
        OptionsJson = MarketingFormRules.NormalizeArrayJson(optionsJson, nameof(optionsJson));
    }
}
