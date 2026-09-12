using Dhole.Content.Domain.Routes.Entities;
using Dhole.Content.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Dhole.Content.UnitTests;

public sealed class Phase4ContentRouteTests
{
    [Theory]
    [InlineData("es/Servicios/", "/es/servicios")]
    [InlineData("/EN//Services", "/en/services")]
    [InlineData("/", "/")]
    public void NormalizePath_ReturnsCanonicalPath(string input, string expected)
        => Assert.Equal(expected, ContentRoute.NormalizePath(input));

    [Fact]
    public void Create_StoresRouteMetadata()
    {
        var contentId = Guid.NewGuid();
        var route = ContentRoute.Create(
            "MAIN",
            contentId,
            "es-CR",
            "es/servicios/",
            true,
            true,
            Guid.NewGuid());

        Assert.Equal("main", route.SiteKey);
        Assert.Equal(contentId, route.ContentId);
        Assert.Equal("es-CR", route.Locale);
        Assert.Equal("/es/servicios", route.Path);
        Assert.True(route.IsPrimary);
        Assert.True(route.IsActive);
        Assert.False(route.IsDeleted);
    }

    [Fact]
    public void Update_CanDeactivateAndChangeCanonicalPath()
    {
        var route = ContentRoute.Create(
            "main",
            Guid.NewGuid(),
            "es-CR",
            "/es/servicios",
            true,
            true,
            null);

        route.Update("main", "es-CR", "/es/servicios-logisticos/", false, false, null);

        Assert.Equal("/es/servicios-logisticos", route.Path);
        Assert.False(route.IsPrimary);
        Assert.False(route.IsActive);
    }

    [Fact]
    public void ContentRouteModel_UsesExpectedUniqueIndexesAndContentForeignKey()
    {
        var options = new DbContextOptionsBuilder<ServiceDbContext>()
            .UseNpgsql("Host=localhost;Database=dhole_content_phase4_test")
            .Options;
        using var db = new ServiceDbContext(options);
        var entity = db.Model.FindEntityType(typeof(ContentRoute));

        Assert.NotNull(entity);

        var pathIndex = entity!.GetIndexes().Single(index =>
            index.Properties.Select(property => property.Name)
                .SequenceEqual(["SiteKey", "Locale", "Path"]));
        Assert.True(pathIndex.IsUnique);
        Assert.Equal("is_deleted = false", pathIndex.GetFilter());

        var primaryIndex = entity.GetIndexes().Single(index =>
            index.Properties.Select(property => property.Name)
                .SequenceEqual(["ContentId", "Locale"]));
        Assert.True(primaryIndex.IsUnique);
        Assert.Equal("is_primary = true AND is_deleted = false", primaryIndex.GetFilter());

        Assert.Contains(entity.GetForeignKeys(), foreignKey =>
            foreignKey.Properties.Select(property => property.Name)
                .SequenceEqual(["ContentId"]));
    }

    [Fact]
    public void ServiceDbContext_ExposesContentRoutes()
    {
        var propertyNames = typeof(ServiceDbContext)
            .GetProperties()
            .Select(property => property.Name)
            .ToHashSet(StringComparer.Ordinal);

        Assert.Contains("ContentRoutes", propertyNames);
    }
}
