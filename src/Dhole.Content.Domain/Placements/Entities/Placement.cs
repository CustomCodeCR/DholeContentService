using CustomCodeFramework.Core.Domain.Entities;

namespace Dhole.Content.Domain.Placements.Entities;

public sealed class Placement : SoftDeletableAggregateRoot<Guid>
{
    private Placement() { }

    private Placement(
        Guid id,
        string siteKey,
        string code,
        string name,
        string allowedTypesJson,
        int maxItems,
        string? settingsJson,
        bool isActive,
        Guid? actorUserId) : base(id)
    {
        Apply(siteKey, code, name, allowedTypesJson, maxItems, settingsJson, isActive);
        MarkAsCreated(DateTime.UtcNow, actorUserId?.ToString());
    }

    public string SiteKey { get; private set; } = "main";
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string AllowedTypesJson { get; private set; } = "[]";
    public int MaxItems { get; private set; }
    public string? SettingsJson { get; private set; }
    public bool IsActive { get; private set; }

    public static Placement Create(
        string siteKey,
        string code,
        string name,
        string allowedTypesJson,
        int maxItems,
        string? settingsJson,
        bool isActive,
        Guid? actorUserId)
        => new(Guid.NewGuid(), siteKey, code, name, allowedTypesJson, maxItems, settingsJson, isActive, actorUserId);

    public void Update(
        string siteKey,
        string code,
        string name,
        string allowedTypesJson,
        int maxItems,
        string? settingsJson,
        bool isActive,
        Guid? actorUserId)
    {
        Apply(siteKey, code, name, allowedTypesJson, maxItems, settingsJson, isActive);
        MarkAsUpdated(DateTime.UtcNow, actorUserId?.ToString());
    }

    public void Delete(Guid? actorUserId)
        => MarkAsDeleted(DateTime.UtcNow, actorUserId?.ToString());

    public bool Allows(Dhole.Content.Domain.ContentItems.Enums.ContentType contentType)
        => PlacementRules.AllowsContentType(AllowedTypesJson, contentType);

    private void Apply(
        string siteKey,
        string code,
        string name,
        string allowedTypesJson,
        int maxItems,
        string? settingsJson,
        bool isActive)
    {
        if (string.IsNullOrWhiteSpace(siteKey)) throw new ArgumentException("SiteKey is required.", nameof(siteKey));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required.", nameof(name));
        if (maxItems <= 0) throw new ArgumentOutOfRangeException(nameof(maxItems), "MaxItems must be greater than zero.");

        SiteKey = siteKey.Trim().ToLowerInvariant();
        Code = PlacementRules.NormalizeCode(code);
        Name = name.Trim();
        AllowedTypesJson = PlacementRules.NormalizeAllowedTypesJson(allowedTypesJson);
        MaxItems = maxItems;
        SettingsJson = PlacementRules.NormalizeSettingsJson(settingsJson);
        IsActive = isActive;
    }
}
