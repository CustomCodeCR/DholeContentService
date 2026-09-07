using CustomCodeFramework.Core.Domain.Entities;
using Dhole.Content.Domain.Media.Events;
namespace Dhole.Content.Domain.Media.Entities;

public sealed class MediaReference : SoftDeletableAggregateRoot<Guid>
{
    private MediaReference() { }
    private MediaReference(Guid id,Guid storageFileId,string fileName,string contentType,string? altText,string? caption,string? metadataJson,Guid? actor):base(id)
    { StorageFileId=storageFileId; FileName=fileName.Trim(); ContentType=contentType.Trim(); AltText=Norm(altText); Caption=Norm(caption); MetadataJson=Norm(metadataJson); MarkAsCreated(DateTime.UtcNow,actor?.ToString()); }
    public Guid StorageFileId { get; private set; }
    public string FileName { get; private set; }=string.Empty;
    public string ContentType { get; private set; }=string.Empty;
    public string? AltText { get; private set; }
    public string? Caption { get; private set; }
    public string? MetadataJson { get; private set; }
    public static MediaReference Create(Guid storageFileId,string fileName,string contentType,string? altText,string? caption,string? metadataJson,Guid? actor)
    { var media=new MediaReference(Guid.NewGuid(),storageFileId,fileName,contentType,altText,caption,metadataJson,actor); media.AddDomainEvent(new MediaReferenceCreatedDomainEvent(media.Id,storageFileId,media.FileName,actor)); return media; }
    public void UpdateMetadata(string? altText,string? caption,string? metadataJson,Guid? actor) { AltText=Norm(altText); Caption=Norm(caption); MetadataJson=Norm(metadataJson); MarkAsUpdated(DateTime.UtcNow,actor?.ToString()); }
    public void Delete(Guid? actor) { MarkAsDeleted(DateTime.UtcNow,actor?.ToString()); AddDomainEvent(new MediaReferenceDeletedDomainEvent(Id,StorageFileId,FileName,actor)); }
    private static string? Norm(string? value)=>string.IsNullOrWhiteSpace(value)?null:value.Trim();
}
