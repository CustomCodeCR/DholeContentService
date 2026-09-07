namespace Dhole.Content.Contracts.ContentItems;
public sealed record SeoWriteRequest(string? Title,string? Description,string? Keywords,string? CanonicalUrl,string? Robots,Guid? OpenGraphMediaId,string? StructuredDataJson);
