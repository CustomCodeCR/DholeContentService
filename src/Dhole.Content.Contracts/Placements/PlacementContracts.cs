namespace Dhole.Content.Contracts.Placements;

public sealed record PlacementDto(
    Guid Id,
    string SiteKey,
    string Code,
    string Name,
    string AllowedTypesJson,
    int MaxItems,
    string? SettingsJson,
    bool IsActive,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);

public sealed record PlacementItemDto(
    Guid Id,
    Guid PlacementId,
    Guid ContentId,
    int SortOrder,
    DateTime? ValidFromUtc,
    DateTime? ValidToUtc,
    string? SettingsJson,
    bool IsActive,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);

public sealed record CreatePlacementRequest(
    string SiteKey,
    string Code,
    string Name,
    string AllowedTypesJson,
    int MaxItems,
    string? SettingsJson,
    bool IsActive = true);

public sealed record UpdatePlacementRequest(
    string SiteKey,
    string Code,
    string Name,
    string AllowedTypesJson,
    int MaxItems,
    string? SettingsJson,
    bool IsActive);

public sealed record CreatePlacementItemRequest(
    Guid ContentId,
    int SortOrder,
    DateTime? ValidFromUtc,
    DateTime? ValidToUtc,
    string? SettingsJson,
    bool IsActive = true);

public sealed record UpdatePlacementItemRequest(
    int SortOrder,
    DateTime? ValidFromUtc,
    DateTime? ValidToUtc,
    string? SettingsJson,
    bool IsActive);
