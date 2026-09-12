using CustomCodeFramework.Core.Domain.Entities;

namespace Dhole.Content.Domain.Placements.Entities;

public sealed class PlacementItem : SoftDeletableAggregateRoot<Guid>
{
    private PlacementItem() { }

    private PlacementItem(
        Guid id,
        Guid placementId,
        Guid contentId,
        int sortOrder,
        DateTime? validFromUtc,
        DateTime? validToUtc,
        string? settingsJson,
        bool isActive,
        Guid? actorUserId) : base(id)
    {
        PlacementId = placementId;
        ContentId = contentId;
        Apply(sortOrder, validFromUtc, validToUtc, settingsJson, isActive);
        MarkAsCreated(DateTime.UtcNow, actorUserId?.ToString());
    }

    public Guid PlacementId { get; private set; }
    public Guid ContentId { get; private set; }
    public int SortOrder { get; private set; }
    public DateTime? ValidFromUtc { get; private set; }
    public DateTime? ValidToUtc { get; private set; }
    public string? SettingsJson { get; private set; }
    public bool IsActive { get; private set; }

    public static PlacementItem Create(
        Guid placementId,
        Guid contentId,
        int sortOrder,
        DateTime? validFromUtc,
        DateTime? validToUtc,
        string? settingsJson,
        bool isActive,
        Guid? actorUserId)
    {
        if (placementId == Guid.Empty) throw new ArgumentException("PlacementId is required.", nameof(placementId));
        if (contentId == Guid.Empty) throw new ArgumentException("ContentId is required.", nameof(contentId));
        return new PlacementItem(Guid.NewGuid(), placementId, contentId, sortOrder, validFromUtc, validToUtc, settingsJson, isActive, actorUserId);
    }

    public void Update(
        int sortOrder,
        DateTime? validFromUtc,
        DateTime? validToUtc,
        string? settingsJson,
        bool isActive,
        Guid? actorUserId)
    {
        Apply(sortOrder, validFromUtc, validToUtc, settingsJson, isActive);
        MarkAsUpdated(DateTime.UtcNow, actorUserId?.ToString());
    }

    public void Delete(Guid? actorUserId)
        => MarkAsDeleted(DateTime.UtcNow, actorUserId?.ToString());

    private void Apply(int sortOrder, DateTime? validFromUtc, DateTime? validToUtc, string? settingsJson, bool isActive)
    {
        if (sortOrder < 0) throw new ArgumentOutOfRangeException(nameof(sortOrder), "SortOrder cannot be negative.");
        if (validFromUtc.HasValue && validToUtc.HasValue && validToUtc.Value <= validFromUtc.Value)
            throw new ArgumentException("ValidToUtc must be later than ValidFromUtc.");

        SortOrder = sortOrder;
        ValidFromUtc = validFromUtc;
        ValidToUtc = validToUtc;
        SettingsJson = PlacementRules.NormalizeSettingsJson(settingsJson);
        IsActive = isActive;
    }
}
