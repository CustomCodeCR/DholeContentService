using System.Text.Json;

namespace Dhole.Content.Domain.Seo;

public sealed record NormalizedSeo(
    string? Title,
    string? Description,
    string? Keywords,
    string? CanonicalUrl,
    string Robots,
    Guid? OpenGraphMediaId,
    string? StructuredDataJson);

public static class SeoRules
{
    public static NormalizedSeo Normalize(
        string? title,
        string? description,
        string? keywords,
        string? canonicalUrl,
        string? robots,
        Guid? openGraphMediaId,
        string? structuredDataJson)
        => new(
            Optional(title),
            Optional(description),
            Optional(keywords),
            NormalizeCanonicalUrl(canonicalUrl),
            NormalizeRobots(robots),
            openGraphMediaId,
            NormalizeStructuredData(structuredDataJson));

    public static string? NormalizeCanonicalUrl(string? value)
    {
        var canonical = Optional(value);
        if (canonical is null) return null;
        if (!Uri.TryCreate(canonical, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp))
            throw new ArgumentException("CanonicalUrl debe ser una URL absoluta HTTP o HTTPS.", nameof(value));
        return uri.AbsoluteUri;
    }

    public static string NormalizeRobots(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return "index,follow";
        var robots = value.Trim().ToLowerInvariant();
        if (robots.Length > 120 || robots.Contains('\r') || robots.Contains('\n'))
            throw new ArgumentException("Robots no es válido.", nameof(value));
        return string.Join(',', robots.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
    }

    public static bool IsIndexable(string? robots)
        => !NormalizeRobots(robots)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Contains("noindex", StringComparer.OrdinalIgnoreCase);

    public static string? NormalizeStructuredData(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        try
        {
            using var document = JsonDocument.Parse(value);
            if (document.RootElement.ValueKind is not (JsonValueKind.Object or JsonValueKind.Array))
                throw new ArgumentException("StructuredDataJson debe ser un objeto o arreglo JSON.", nameof(value));
            return document.RootElement.GetRawText();
        }
        catch (JsonException exception)
        {
            throw new ArgumentException("StructuredDataJson debe contener JSON válido.", nameof(value), exception);
        }
    }

    private static string? Optional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
