namespace Dhole.Content.Contracts.Collections;

public sealed record CollectionDto(
    Guid Id,
    string SiteKey,
    string Code,
    string Name,
    string? SettingsJson,
    bool IsActive,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);

public sealed record CollectionItemDto(
    Guid Id,
    Guid CollectionId,
    string DataJson,
    int SortOrder,
    bool IsActive,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);

public sealed record CreateCollectionRequest(
    string SiteKey,
    string Code,
    string Name,
    string? SettingsJson,
    bool IsActive = true);

public sealed record UpdateCollectionRequest(
    string SiteKey,
    string Code,
    string Name,
    string? SettingsJson,
    bool IsActive);

public sealed record CreateCollectionItemRequest(
    string DataJson,
    int SortOrder,
    bool IsActive = true);

public sealed record UpdateCollectionItemRequest(
    string DataJson,
    int SortOrder,
    bool IsActive);
