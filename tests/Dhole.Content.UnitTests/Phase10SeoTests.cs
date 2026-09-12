using Dhole.Content.Domain.Seo;
using Xunit;

namespace Dhole.Content.UnitTests;

public sealed class Phase10SeoTests
{
    [Fact]
    public void NormalizeCanonicalUrl_AcceptsAbsoluteHttpUrl()
        => Assert.Equal("https://logisticacastrofallas.com/servicios", SeoRules.NormalizeCanonicalUrl("https://logisticacastrofallas.com/servicios"));

    [Theory]
    [InlineData("/servicios")]
    [InlineData("ftp://example.com/page")]
    public void NormalizeCanonicalUrl_RejectsUnsupportedUrls(string value)
        => Assert.Throws<ArgumentException>(() => SeoRules.NormalizeCanonicalUrl(value));

    [Fact]
    public void NormalizeRobots_NormalizesAndDetectsNoIndex()
    {
        Assert.Equal("noindex,follow", SeoRules.NormalizeRobots(" NoIndex, Follow "));
        Assert.False(SeoRules.IsIndexable("noindex,follow"));
        Assert.True(SeoRules.IsIndexable("index,follow"));
    }

    [Fact]
    public void NormalizeStructuredData_RequiresJsonObjectOrArray()
    {
        Assert.Equal("{\"@type\":\"Organization\"}", SeoRules.NormalizeStructuredData("{\"@type\":\"Organization\"}"));
        Assert.Throws<ArgumentException>(() => SeoRules.NormalizeStructuredData("not-json"));
        Assert.Throws<ArgumentException>(() => SeoRules.NormalizeStructuredData("\"text\""));
    }

    [Fact]
    public void SitemapBuilder_GeneratesStandardXmlAndEscapesUrls()
    {
        var xml = SeoDocumentBuilder.BuildSitemap([
            new SitemapDocumentEntry("https://example.com/noticias?a=1&b=2", new DateTime(2026, 9, 12, 12, 0, 0, DateTimeKind.Utc), 0.8m, "weekly")
        ]);
        Assert.Contains("http://www.sitemaps.org/schemas/sitemap/0.9", xml);
        Assert.Contains("https://example.com/noticias?a=1&amp;b=2", xml);
        Assert.Contains("<lastmod>2026-09-12</lastmod>", xml);
        Assert.Contains("<changefreq>weekly</changefreq>", xml);
        Assert.Contains("<priority>0.8</priority>", xml);
    }

    [Fact]
    public void RobotsBuilder_ReferencesPublicSitemap()
    {
        var text = SeoDocumentBuilder.BuildRobots("https://logisticacastrofallas.com");
        Assert.Contains("User-agent: *", text);
        Assert.Contains("Allow: /", text);
        Assert.Contains("Sitemap: https://logisticacastrofallas.com/sitemap.xml", text);
    }
}
