using Dhole.Content.Domain.ContentItems.Entities;
using Dhole.Content.Domain.ContentItems.Enums;
using Xunit;

namespace Dhole.Content.UnitTests;

public sealed class MasterPhase3ContentExperienceTests
{
    [Theory]
    [InlineData(ContentType.Page)]
    [InlineData(ContentType.Post)]
    [InlineData(ContentType.News)]
    public void PageArticleAndNews_UseTheSharedContentAggregate(ContentType type)
    {
        var actor = Guid.NewGuid();
        var featuredMediaId = Guid.NewGuid();
        var taxonomyId = Guid.NewGuid();
        var item = ContentItem.Create(type, "Título", "titulo", "[]", null, actor, "main", "es-CR");

        item.Update(
            "Título actualizado",
            "titulo-actualizado",
            "Resumen editorial",
            "[]",
            "<p>Contenido</p>",
            featuredMediaId,
            10,
            true,
            "es-CR",
            actor);
        item.ReplaceTaxonomies([taxonomyId]);
        item.SetSeo(
            "SEO title",
            "SEO description",
            "cms,marketing",
            "https://example.test/titulo-actualizado",
            "index,follow",
            featuredMediaId,
            null,
            actor);

        Assert.Equal(type, item.Type);
        Assert.Equal("Resumen editorial", item.Excerpt);
        Assert.Equal(featuredMediaId, item.FeaturedMediaId);
        Assert.Contains(item.Taxonomies, x => x.TaxonomyTermId == taxonomyId);
        Assert.Equal("SEO title", item.SeoTitle);
        Assert.Equal(ContentStatus.Draft, item.Status);
    }

    [Fact]
    public void ArticleRevision_PreservesEditorialSnapshotBeforeFurtherChanges()
    {
        var actor = Guid.NewGuid();
        var item = ContentItem.Create(ContentType.Post, "Artículo original", "articulo-original", "[]", null, actor);
        item.Update(
            "Artículo original",
            "articulo-original",
            "Resumen original",
            "[]",
            "<p>Versión original</p>",
            null,
            0,
            false,
            "es-CR",
            actor);
        item.SetSeo("SEO original", "Descripción", null, null, "index,follow", null, null, actor);

        var revision = item.CreateRevision(actor, "before-update");
        item.Update(
            "Artículo nuevo",
            "articulo-nuevo",
            "Resumen nuevo",
            "[]",
            "<p>Versión nueva</p>",
            null,
            0,
            false,
            "es-CR",
            actor);

        Assert.Equal("Artículo original", revision.Title);
        Assert.Equal("articulo-original", revision.Slug);
        Assert.Equal("Resumen original", revision.Excerpt);
        Assert.Equal("<p>Versión original</p>", revision.RenderedHtml);
        Assert.Equal("SEO original", revision.SeoTitle);
        Assert.Equal("Artículo nuevo", item.Title);
    }

    [Fact]
    public void ScheduledArticle_FollowsTheSameReviewedLifecycleAsPageAndNews()
    {
        var actor = Guid.NewGuid();
        var now = new DateTime(2026, 9, 16, 18, 0, 0, DateTimeKind.Utc);
        var publishAt = now.AddHours(3);
        var item = ContentItem.Create(ContentType.Post, "Artículo programado", "articulo-programado", "[]", null, actor);

        item.SubmitForReview(actor);
        item.Schedule(publishAt, null, now, actor);

        Assert.Equal(ContentStatus.Scheduled, item.Status);
        Assert.Equal(publishAt, item.ScheduledAtUtc);
    }
}
