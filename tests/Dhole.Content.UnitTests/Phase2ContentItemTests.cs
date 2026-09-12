using Dhole.Content.Domain.ContentItems.Entities;
using Dhole.Content.Domain.ContentItems.Enums;
using Dhole.Content.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Dhole.Content.UnitTests;

public sealed class Phase2ContentItemTests
{
    [Fact]
    public void ConfigureCmsMetadata_StoresPhase2Fields()
    {
        var actor = Guid.NewGuid();
        var parentId = Guid.NewGuid();
        var translationGroupId = Guid.NewGuid();
        var unpublishAtUtc = DateTime.UtcNow.AddDays(30);
        var item = ContentItem.Create(
            ContentType.Page,
            "Servicios",
            "servicios",
            "[]",
            null,
            actor,
            "main",
            "es-CR");

        item.ConfigureCmsMetadata(
            parentId,
            translationGroupId,
            "landing-page",
            unpublishAtUtc,
            0.8m,
            "Weekly",
            actor);

        Assert.Equal(parentId, item.ParentContentId);
        Assert.Equal(translationGroupId, item.TranslationGroupId);
        Assert.Equal("landing-page", item.TemplateKey);
        Assert.Equal(unpublishAtUtc, item.UnpublishAtUtc);
        Assert.Equal(0.8m, item.SitemapPriority);
        Assert.Equal("weekly", item.SitemapChangeFrequency);
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(1.01)]
    public void ConfigureCmsMetadata_RejectsInvalidSitemapPriority(double value)
    {
        var item = ContentItem.Create(
            ContentType.Page,
            "Servicios",
            "servicios",
            "[]",
            null,
            Guid.NewGuid());

        Assert.Throws<ArgumentOutOfRangeException>(() => item.ConfigureCmsMetadata(
            null,
            null,
            null,
            null,
            (decimal)value,
            null,
            null));
    }

    [Fact]
    public void ContentItemModel_UsesSiteLocaleSlugAsUniqueKey()
    {
        var options = new DbContextOptionsBuilder<ServiceDbContext>()
            .UseNpgsql("Host=localhost;Database=dhole_content_phase2_test")
            .Options;
        using var db = new ServiceDbContext(options);
        var entity = db.Model.FindEntityType(typeof(ContentItem));

        Assert.NotNull(entity);
        var localizedSlugIndex = entity!.GetIndexes().Single(index =>
            index.Properties.Select(property => property.Name)
                .SequenceEqual(["SiteKey", "Locale", "Slug"]));

        Assert.True(localizedSlugIndex.IsUnique);
        Assert.DoesNotContain(entity.GetIndexes(), index =>
            index.Properties.Select(property => property.Name)
                .SequenceEqual(["SiteKey", "Slug"]));
    }
}
