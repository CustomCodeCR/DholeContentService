namespace Dhole.Content.Contracts.Media;
public sealed record RegisterMediaRequest(Guid StorageFileId,string FileName,string ContentType,string? AltText,string? Caption,string? MetadataJson);
