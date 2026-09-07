namespace Dhole.Content.Contracts.ContentItems;
public sealed record ContentRevisionDto(Guid Id,int RevisionNumber,string Title,string Slug,string? Reason,Guid? CreatedBy,DateTime CreatedAtUtc);
