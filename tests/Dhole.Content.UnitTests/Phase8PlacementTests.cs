using Dhole.Content.Domain.ContentItems.Enums;
using Dhole.Content.Domain.Placements;
using Dhole.Content.Domain.Placements.Entities;
using Dhole.Content.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Dhole.Content.UnitTests;

public sealed class Phase8PlacementTests
{
    [Fact]
    public void Placement_NormalizesCodeAndAllowedTypes()
    {
        var placement = Placement.Create(" MAIN ", " Home.Hero ", "Hero", "[\"banner\",\"News\",\"Banner\"]", 3,
            "{\"layout\":\"carousel\"}", true, Guid.NewGuid());

        Assert.Equal("main", placement.SiteKey);
        Assert.Equal("home.hero", placement.Code);
        Assert.Equal("[\"Banner\",\"News\"]", placement.AllowedTypesJson);
        Assert.True(placement.Allows(ContentType.Banner));
        Assert.True(placement.Allows(ContentType.News));
        Assert.False(placement.Allows(ContentType.Page));
    }

    [Fact]
    public void Placement_RejectsUnknownContentType()
        => Assert.Throws<ArgumentException>(() => Placement.Create("main", "home.hero", "Hero", "[\"Whatever\"]", 1, null, true, null));

    [Fact]
    public void Placement_RejectsNonPositiveMaxItems()
        => Assert.Throws<ArgumentOutOfRangeException>(() => Placement.Create("main", "home.hero", "Hero", "[\"Banner\"]", 0, null, true, null));

    [Fact]
    public void PlacementItem_RejectsInvalidValidityWindow()
    {
        var from = DateTime.UtcNow.AddDays(1);
        Assert.Throws<ArgumentException>(() => PlacementItem.Create(Guid.NewGuid(), Guid.NewGuid(), 0, from, from.AddMinutes(-1), null, true, null));
    }

    [Fact]
    public void PlacementItem_StoresSchedulingAndSettings()
    {
        var from = DateTime.UtcNow.AddDays(1);
        var to = from.AddDays(2);
        var item = PlacementItem.Create(Guid.NewGuid(), Guid.NewGuid(), 4, from, to, "{\"variant\":\"dark\"}", false, null);
        Assert.Equal(4, item.SortOrder);
        Assert.Equal(from, item.ValidFromUtc);
        Assert.Equal(to, item.ValidToUtc);
        Assert.False(item.IsActive);
    }

    [Fact]
    public void PlacementModel_HasExpectedIndexesAndForeignKeys()
    {
        var options = new DbContextOptionsBuilder<ServiceDbContext>()
            .UseNpgsql("Host=localhost;Database=dhole_content_phase8_test")
            .Options;
        using var db = new ServiceDbContext(options);

        var placement = db.Model.FindEntityType(typeof(Placement));
        Assert.NotNull(placement);
        var codeIndex = placement!.GetIndexes().Single(index => index.Properties.Select(property => property.Name)
            .SequenceEqual(["SiteKey", "Code"]));
        Assert.True(codeIndex.IsUnique);
        Assert.Equal("is_deleted = false", codeIndex.GetFilter());

        var item = db.Model.FindEntityType(typeof(PlacementItem));
        Assert.NotNull(item);
        var associationIndex = item!.GetIndexes().Single(index => index.Properties.Select(property => property.Name)
            .SequenceEqual(["PlacementId", "ContentId"]));
        Assert.True(associationIndex.IsUnique);
        Assert.Equal("is_deleted = false", associationIndex.GetFilter());
        Assert.Contains(item.GetForeignKeys(), foreignKey => foreignKey.Properties.Select(property => property.Name).SequenceEqual(["PlacementId"]));
        Assert.Contains(item.GetForeignKeys(), foreignKey => foreignKey.Properties.Select(property => property.Name).SequenceEqual(["ContentId"]));
    }

    [Fact]
    public void ServiceDbContext_ExposesPlacementsAndPlacementItems()
    {
        var propertyNames = typeof(ServiceDbContext).GetProperties().Select(property => property.Name).ToHashSet(StringComparer.Ordinal);
        Assert.Contains("Placements", propertyNames);
        Assert.Contains("PlacementItems", propertyNames);
    }
}
