namespace Dhole.Content.Contracts.Media;

public sealed record ContentMediaDto(
    Guid Id,
    Guid ContentId,
    Guid MediaReferenceId,
    string Role,
    int SortOrder,
    string? AltTextOverride,
    string? CaptionOverride,
    decimal? FocalX,
    decimal? FocalY,
    string? SettingsJson,
    Guid StorageFileId,
    string FileName,
    string ContentType,
    string? AltText,
    string? Caption);

public sealed record CreateContentMediaRequest(
    Guid MediaReferenceId,
    string Role,
    int SortOrder,
    string? AltTextOverride,
    string? CaptionOverride,
    decimal? FocalX,
    decimal? FocalY,
    string? SettingsJson);

public sealed record UpdateContentMediaRequest(
    string Role,
    int SortOrder,
    string? AltTextOverride,
    string? CaptionOverride,
    decimal? FocalX,
    decimal? FocalY,
    string? SettingsJson);
