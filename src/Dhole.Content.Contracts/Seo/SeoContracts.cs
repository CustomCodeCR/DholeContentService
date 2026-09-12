namespace Dhole.Content.Contracts.Seo;

public sealed record UpdateSeoRequest(
    string? Title,
    string? Description,
    string? Keywords,
    string? CanonicalUrl,
    string? Robots,
    Guid? OpenGraphMediaId,
    string? StructuredDataJson);

public sealed record SeoPlatformPreviewDto(
    string Title,
    string? Description,
    string Url,
    Guid? ImageMediaId,
    string CardType);

public sealed record SeoPreviewDto(
    Guid ContentId,
    string Robots,
    string? Keywords,
    string? StructuredDataJson,
    SeoPlatformPreviewDto Google,
    SeoPlatformPreviewDto Facebook,
    SeoPlatformPreviewDto LinkedIn,
    SeoPlatformPreviewDto Twitter);
