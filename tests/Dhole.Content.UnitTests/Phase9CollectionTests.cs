using Dhole.Content.Domain.Collections;
using Dhole.Content.Domain.Collections.Entities;
using Dhole.Content.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Dhole.Content.UnitTests;

public sealed class Phase9CollectionTests
{
    [Fact]
    public void Collection_Create_NormalizesSiteAndCode()
    {
        var collection = ContentCollection.Create(" MAIN ", " FAQ.Home ", "Preguntas frecuentes", "{\"layout\":\"accordion\"}", true, Guid.NewGuid());

        Assert.Equal("main", collection.SiteKey);
        Assert.Equal("faq.home", collection.Code);
        Assert.Equal("Preguntas frecuentes", collection.Name);
        Assert.Equal("{\"layout\":\"accordion\"}", collection.SettingsJson);
        Assert.True(collection.IsActive);
    }

    [Fact]
    public void CollectionRules_RejectsInvalidCode()
        => Assert.Throws<ArgumentException>(() => CollectionRules.NormalizeCode("faq home<script>"));

    [Fact]
    public void CollectionItem_Create_NormalizesDataJson()
    {
        var item = ContentCollectionItem.Create(Guid.NewGuid(), "{ \"question\": \"¿Qué hacemos?\", \"answer\": \"Logística\" }", 3, true, null);

        Assert.Equal("{ \"question\": \"¿Qué hacemos?\", \"answer\": \"Logística\" }", item.DataJson);
        Assert.Equal(3, item.SortOrder);
        Assert.True(item.IsActive);
    }

    [Theory]
    [InlineData("[]")]
    [InlineData("\"text\"")]
    [InlineData("not-json")]
    public void CollectionItem_RejectsNonObjectJson(string dataJson)
        => Assert.Throws<ArgumentException>(() => ContentCollectionItem.Create(Guid.NewGuid(), dataJson, 0, true, null));

    [Fact]
    public void CollectionItem_RejectsNegativeSortOrder()
        => Assert.Throws<ArgumentOutOfRangeException>(() => ContentCollectionItem.Create(Guid.NewGuid(), "{}", -1, true, null));

    [Fact]
    public void CollectionModel_HasExpectedIndexesAndForeignKey()
    {
        var options = new DbContextOptionsBuilder<ServiceDbContext>()
            .UseNpgsql("Host=localhost;Database=dhole_content_phase9_test")
            .Options;
        using var db = new ServiceDbContext(options);

        var collection = db.Model.FindEntityType(typeof(ContentCollection));
        var item = db.Model.FindEntityType(typeof(ContentCollectionItem));
        Assert.NotNull(collection);
        Assert.NotNull(item);

        var uniqueCode = collection!.GetIndexes().Single(index =>
            index.Properties.Select(property => property.Name).SequenceEqual(["SiteKey", "Code"]));
        Assert.True(uniqueCode.IsUnique);
        Assert.Equal("is_deleted = false", uniqueCode.GetFilter());

        var sortIndex = item!.GetIndexes().Single(index =>
            index.Properties.Select(property => property.Name).SequenceEqual(["CollectionId", "SortOrder"]));
        Assert.False(sortIndex.IsUnique);
        Assert.Equal("is_deleted = false", sortIndex.GetFilter());

        Assert.Contains(item.GetForeignKeys(), foreignKey =>
            foreignKey.Properties.Select(property => property.Name).SequenceEqual(["CollectionId"]));
    }

    [Fact]
    public void ServiceDbContext_ExposesCollections()
    {
        var propertyNames = typeof(ServiceDbContext).GetProperties().Select(property => property.Name).ToHashSet(StringComparer.Ordinal);
        Assert.Contains("Collections", propertyNames);
        Assert.Contains("CollectionItems", propertyNames);
    }
}
