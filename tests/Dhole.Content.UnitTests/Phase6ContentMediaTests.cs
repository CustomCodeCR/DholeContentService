using Dhole.Content.Domain.Media;
using Dhole.Content.Domain.Media.Entities;
using Dhole.Content.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Dhole.Content.UnitTests;

public sealed class Phase6ContentMediaTests
{
    [Fact]
    public void ContentMediaRoles_ContainsExpectedRoles()
    {
        Assert.Equal(
            [
                "hero",
                "featured",
                "gallery",
                "thumbnail",
                "background",
                "video",
                "video.poster",
                "banner.desktop",
                "banner.mobile",
                "open-graph"
            ],
            ContentMediaRoles.All);
    }

    [Fact]
    public void Create_NormalizesRoleAndStoresOverrides()
    {
        var contentId = Guid.NewGuid();
        var mediaId = Guid.NewGuid();
        var link = ContentMedia.Create(
            contentId,
            mediaId,
            " HERO ",
            2,
            "Hero alt",
            "Hero caption",
            0.25m,
            0.75m,
            "{\"fit\":\"cover\"}",
            Guid.NewGuid());

        Assert.Equal(contentId, link.ContentId);
        Assert.Equal(mediaId, link.MediaReferenceId);
        Assert.Equal("hero", link.Role);
        Assert.Equal(2, link.SortOrder);
        Assert.Equal("Hero alt", link.AltTextOverride);
        Assert.Equal("Hero caption", link.CaptionOverride);
        Assert.Equal(0.25m, link.FocalX);
        Assert.Equal(0.75m, link.FocalY);
        Assert.Equal("{\"fit\":\"cover\"}", link.SettingsJson);
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(1.01)]
    public void Create_RejectsFocalCoordinatesOutsideNormalizedRange(double value)
    {
        var focal = (decimal)value;
        Assert.Throws<ArgumentOutOfRangeException>(() => ContentMedia.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "gallery",
            0,
            null,
            null,
            focal,
            null,
            null,
            null));
    }

    [Fact]
    public void Create_RejectsUnsupportedRole()
        => Assert.Throws<ArgumentException>(() => ContentMedia.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "custom-role",
            0,
            null,
            null,
            null,
            null,
            null,
            null));

    [Fact]
    public void Create_RejectsInvalidSettingsJson()
        => Assert.Throws<ArgumentException>(() => ContentMedia.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "gallery",
            0,
            null,
            null,
            null,
            null,
            "not-json",
            null));

    [Fact]
    public void ContentMediaModel_HasExpectedIndexesAndForeignKeys()
    {
        var options = new DbContextOptionsBuilder<ServiceDbContext>()
            .UseNpgsql("Host=localhost;Database=dhole_content_phase6_test")
            .Options;
        using var db = new ServiceDbContext(options);
        var entity = db.Model.FindEntityType(typeof(ContentMedia));

        Assert.NotNull(entity);

        var uniqueAssociation = entity!.GetIndexes().Single(index =>
            index.Properties.Select(property => property.Name)
                .SequenceEqual(["ContentId", "MediaReferenceId", "Role"]));
        Assert.True(uniqueAssociation.IsUnique);
        Assert.Equal("is_deleted = false", uniqueAssociation.GetFilter());

        var sortIndex = entity.GetIndexes().Single(index =>
            index.Properties.Select(property => property.Name)
                .SequenceEqual(["ContentId", "Role", "SortOrder"]));
        Assert.False(sortIndex.IsUnique);
        Assert.Equal("is_deleted = false", sortIndex.GetFilter());

        Assert.Contains(entity.GetForeignKeys(), foreignKey =>
            foreignKey.Properties.Select(property => property.Name).SequenceEqual(["ContentId"]));
        Assert.Contains(entity.GetForeignKeys(), foreignKey =>
            foreignKey.Properties.Select(property => property.Name).SequenceEqual(["MediaReferenceId"]));
    }

    [Fact]
    public void ServiceDbContext_ExposesContentMedia()
    {
        var propertyNames = typeof(ServiceDbContext)
            .GetProperties()
            .Select(property => property.Name)
            .ToHashSet(StringComparer.Ordinal);

        Assert.Contains("ContentMedia", propertyNames);
    }
}
