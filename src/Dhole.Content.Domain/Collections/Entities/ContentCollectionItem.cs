using CustomCodeFramework.Core.Domain.Entities;

namespace Dhole.Content.Domain.Collections.Entities;

public sealed class ContentCollectionItem : SoftDeletableAggregateRoot<Guid>
{
    private ContentCollectionItem() { }

    private ContentCollectionItem(
        Guid id,
        Guid collectionId,
        string dataJson,
        int sortOrder,
        bool isActive,
        Guid? actorUserId) : base(id)
    {
        CollectionId = collectionId;
        Apply(dataJson, sortOrder, isActive);
        MarkAsCreated(DateTime.UtcNow, actorUserId?.ToString());
    }

    public Guid CollectionId { get; private set; }
    public string DataJson { get; private set; } = "{}";
    public int SortOrder { get; private set; }
    public bool IsActive { get; private set; }

    public static ContentCollectionItem Create(
        Guid collectionId,
        string dataJson,
        int sortOrder,
        bool isActive,
        Guid? actorUserId)
    {
        if (collectionId == Guid.Empty) throw new ArgumentException("CollectionId is required.", nameof(collectionId));
        return new ContentCollectionItem(Guid.NewGuid(), collectionId, dataJson, sortOrder, isActive, actorUserId);
    }

    public void Update(string dataJson, int sortOrder, bool isActive, Guid? actorUserId)
    {
        Apply(dataJson, sortOrder, isActive);
        MarkAsUpdated(DateTime.UtcNow, actorUserId?.ToString());
    }

    public void Delete(Guid? actorUserId)
        => MarkAsDeleted(DateTime.UtcNow, actorUserId?.ToString());

    private void Apply(string dataJson, int sortOrder, bool isActive)
    {
        if (sortOrder < 0) throw new ArgumentOutOfRangeException(nameof(sortOrder), "SortOrder cannot be negative.");
        DataJson = CollectionRules.NormalizeDataJson(dataJson);
        SortOrder = sortOrder;
        IsActive = isActive;
    }
}
