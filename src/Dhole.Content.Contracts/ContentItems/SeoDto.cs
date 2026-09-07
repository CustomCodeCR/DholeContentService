namespace Dhole.Content.Contracts.ContentItems;
public sealed record SeoDto(string? Title,string? Description,string? Keywords,string? CanonicalUrl,string? Robots,Guid? OpenGraphMediaId,string? StructuredDataJson);
