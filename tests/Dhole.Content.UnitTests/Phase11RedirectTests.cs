using Dhole.Content.Domain.Redirects.Entities;
using Dhole.Content.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Dhole.Content.UnitTests;

public sealed class Phase11RedirectTests
{
    [Fact]
    public void Redirect_Create_NormalizesSiteAndSourcePath()
    {
        var redirect = ContentRedirect.Create(" MAIN ", "Servicios//Viejo/", "/servicios/nuevo", 301, true, null, null, null);
        Assert.Equal("main", redirect.SiteKey);
        Assert.Equal("/servicios/viejo", redirect.SourcePath);
        Assert.Equal("/servicios/nuevo", redirect.TargetUrl);
    }

    [Theory]
    [InlineData(301)]
    [InlineData(302)]
    public void Redirect_AllowsSupportedStatusCodes(int statusCode)
        => Assert.Equal(statusCode, ContentRedirect.Create("main", "/a", "/b", statusCode, true, null, null, null).StatusCode);

    [Fact]
    public void Redirect_RejectsUnsupportedStatusCode()
        => Assert.Throws<ArgumentOutOfRangeException>(() => ContentRedirect.Create("main", "/a", "/b", 307, true, null, null, null));

    [Fact]
    public void Redirect_RejectsSelfRedirect()
        => Assert.Throws<ArgumentException>(() => ContentRedirect.Create("main", "/old", "/OLD/", 301, true, null, null, null));

    [Fact]
    public void Redirect_RespectsValidityWindow()
    {
        var now = DateTime.UtcNow;
        var redirect = ContentRedirect.Create("main", "/a", "/b", 302, true, now.AddMinutes(-5), now.AddMinutes(5), null);
        Assert.True(redirect.IsEffectiveAt(now));
        Assert.False(redirect.IsEffectiveAt(now.AddMinutes(6)));
    }

    [Fact]
    public void RedirectModel_HasUniqueSourcePathIndex()
    {
        var options = new DbContextOptionsBuilder<ServiceDbContext>()
            .UseNpgsql("Host=localhost;Database=dhole_content_phase11_test")
            .Options;
        using var db = new ServiceDbContext(options);
        var entity = db.Model.FindEntityType(typeof(ContentRedirect));
        Assert.NotNull(entity);
        var index = entity!.GetIndexes().Single(x => x.Properties.Select(p => p.Name).SequenceEqual(["SiteKey", "SourcePath"]));
        Assert.True(index.IsUnique);
        Assert.Equal("is_deleted = false", index.GetFilter());
    }

    [Fact]
    public void ServiceDbContext_ExposesRedirects()
        => Assert.Contains("Redirects", typeof(ServiceDbContext).GetProperties().Select(property => property.Name));
}
