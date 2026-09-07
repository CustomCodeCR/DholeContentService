namespace Dhole.Content.Contracts.Media;
public sealed record MediaDto(Guid Id,Guid StorageFileId,string FileName,string ContentType,string? AltText,string? Caption,string? MetadataJson,DateTime CreatedAtUtc,DateTime? UpdatedAtUtc);
