using System.Globalization;
using System.Xml.Linq;

namespace Dhole.Content.Domain.Seo;

public sealed record SitemapDocumentEntry(
    string Url,
    DateTime? LastModifiedUtc,
    decimal? Priority,
    string? ChangeFrequency);

public static class SeoDocumentBuilder
{
    private static readonly XNamespace SitemapNamespace = "http://www.sitemaps.org/schemas/sitemap/0.9";

    public static string BuildSitemap(IEnumerable<SitemapDocumentEntry> entries)
    {
        var urlSet = new XElement(SitemapNamespace + "urlset");
        foreach (var entry in entries)
        {
            var url = new XElement(SitemapNamespace + "url", new XElement(SitemapNamespace + "loc", entry.Url));
            if (entry.LastModifiedUtc.HasValue)
                url.Add(new XElement(SitemapNamespace + "lastmod", entry.LastModifiedUtc.Value.ToUniversalTime().ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)));
            if (!string.IsNullOrWhiteSpace(entry.ChangeFrequency))
                url.Add(new XElement(SitemapNamespace + "changefreq", entry.ChangeFrequency));
            if (entry.Priority.HasValue)
                url.Add(new XElement(SitemapNamespace + "priority", entry.Priority.Value.ToString("0.0#", CultureInfo.InvariantCulture)));
            urlSet.Add(url);
        }

        return new XDocument(new XDeclaration("1.0", "utf-8", null), urlSet).ToString(SaveOptions.DisableFormatting);
    }

    public static string BuildRobots(string publicBaseUrl)
        => $"User-agent: *\nAllow: /\nSitemap: {publicBaseUrl.TrimEnd('/')}/sitemap.xml\n";
}
