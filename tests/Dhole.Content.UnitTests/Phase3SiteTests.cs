using Dhole.Content.Domain.Sites.Entities;
using Dhole.Content.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Dhole.Content.UnitTests;

public sealed class Phase3SiteTests
{
    [Fact]
    public void Create_NormalizesSiteKeyAndPrimaryDomain()
    {
        var logo = Guid.NewGuid();
        var favicon = Guid.NewGuid();
        var openGraph = Guid.NewGuid();

        var site = Site.Create(
            " MAIN ",
            "Logística Castro Fallas",
            "https://LogisticaCastroFallas.com/",
            "es-CR",
            "America/Costa_Rica",
            logo,
            favicon,
            openGraph,
            "Active",
            Guid.NewGuid()
        );

        Assert.Equal("main", site.SiteKey);
        Assert.Equal("logisticacastrofallas.com", site.PrimaryDomain);
        Assert.Equal("es-CR", site.DefaultLocale);
        Assert.Equal("America/Costa_Rica", site.TimeZone);
        Assert.Equal(logo, site.LogoMediaId);
        Assert.Equal(favicon, site.FaviconMediaId);
        Assert.Equal(openGraph, site.DefaultOpenGraphMediaId);
        Assert.Equal("Active", site.Status);
    }

    [Fact]
    public void SiteModel_MapsSitesAndUniqueIdentifiers()
    {
        var options = new DbContextOptionsBuilder<ServiceDbContext>()
            .UseNpgsql("Host=localhost;Database=dhole_content_phase3_test")
            .Options;
        using var db = new ServiceDbContext(options);
        var entity = db.Model.FindEntityType(typeof(Site));

        Assert.NotNull(entity);
        Assert.Equal("sites", entity!.GetTableName());
        Assert.Contains(entity.GetIndexes(), index =>
            index.IsUnique && index.Properties.Select(property => property.Name).SequenceEqual(["SiteKey"]));
        Assert.Contains(entity.GetIndexes(), index =>
            index.IsUnique && index.Properties.Select(property => property.Name).SequenceEqual(["PrimaryDomain"]));
        Assert.NotNull(typeof(ServiceDbContext).GetProperty("Sites"));
    }

    [Fact]
    public void Update_PreservesSiteKeyAndChangesSiteConfiguration()
    {
        var site = Site.Create(
            "main",
            "Sitio principal",
            "example.com",
            "es-CR",
            "America/Costa_Rica",
            null,
            null,
            null,
            "Active",
            null
        );

        site.Update(
            "Main site",
            "www.example.com",
            "en-US",
            "America/New_York",
            null,
            null,
            null,
            "Maintenance",
            Guid.NewGuid()
        );

        Assert.Equal("main", site.SiteKey);
        Assert.Equal("Main site", site.Name);
        Assert.Equal("www.example.com", site.PrimaryDomain);
        Assert.Equal("en-US", site.DefaultLocale);
        Assert.Equal("America/New_York", site.TimeZone);
        Assert.Equal("Maintenance", site.Status);
    }
}
