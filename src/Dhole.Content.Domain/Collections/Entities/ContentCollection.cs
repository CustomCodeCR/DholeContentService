using CustomCodeFramework.Core.Domain.Entities;

namespace Dhole.Content.Domain.Collections.Entities;

public sealed class ContentCollection : SoftDeletableAggregateRoot<Guid>
{
    private ContentCollection() { }

    private ContentCollection(
        Guid id,
        string siteKey,
        string code,
        string name,
        string? settingsJson,
        bool isActive,
        Guid? actorUserId) : base(id)
    {
        Apply(siteKey, code, name, settingsJson, isActive);
        MarkAsCreated(DateTime.UtcNow, actorUserId?.ToString());
    }

    public string SiteKey { get; private set; } = "main";
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? SettingsJson { get; private set; }
    public bool IsActive { get; private set; }

    public static ContentCollection Create(
        string siteKey,
        string code,
        string name,
        string? settingsJson,
        bool isActive,
        Guid? actorUserId)
        => new(Guid.NewGuid(), siteKey, code, name, settingsJson, isActive, actorUserId);

    public void Update(
        string siteKey,
        string code,
        string name,
        string? settingsJson,
        bool isActive,
        Guid? actorUserId)
    {
        Apply(siteKey, code, name, settingsJson, isActive);
        MarkAsUpdated(DateTime.UtcNow, actorUserId?.ToString());
    }

    public void Delete(Guid? actorUserId)
        => MarkAsDeleted(DateTime.UtcNow, actorUserId?.ToString());

    private void Apply(string siteKey, string code, string name, string? settingsJson, bool isActive)
    {
        if (string.IsNullOrWhiteSpace(siteKey)) throw new ArgumentException("SiteKey is required.", nameof(siteKey));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required.", nameof(name));

        SiteKey = siteKey.Trim().ToLowerInvariant();
        Code = CollectionRules.NormalizeCode(code);
        Name = name.Trim();
        SettingsJson = CollectionRules.NormalizeSettingsJson(settingsJson);
        IsActive = isActive;
    }
}
