using Dhole.Content.Application.Abstractions.Cache;
using Dhole.Content.Infrastructure.Cache;
using Xunit;

namespace Dhole.Content.UnitTests;

public sealed class Phase31RedisCacheTests
{
    [Fact]
    public void PublicCacheKeys_AreVersionedPerSite()
    {
        const string site = "main";
        var contentV1 = ContentCacheKeys.PublishedContent(site, "v1", "home", "es-CR");
        var contentV2 = ContentCacheKeys.PublishedContent(site, "v2", "home", "es-CR");
        var routeV1 = ContentCacheKeys.PublishedRoute(site, "v1", "es-CR", "/");
        var routeV2 = ContentCacheKeys.PublishedRoute(site, "v2", "es-CR", "/");

        Assert.NotEqual(contentV1, contentV2);
        Assert.NotEqual(routeV1, routeV2);
        Assert.StartsWith("cms:public:main:v:v1:", contentV1);
        Assert.Equal("cms:public:main:generation", ContentCacheKeys.Generation(site));
    }

    [Fact]
    public void Phase31CacheContract_CoversAllPublicReadFamilies()
    {
        var contract = typeof(IContentCacheService);
        foreach (var method in new[]
        {
            "GetPublishedContentAsync", "GetPublishedRouteAsync", "GetMenuAsync",
            "GetPublicPlacementsAsync", "GetPublicPlacementItemsAsync",
            "GetPublicCollectionsAsync", "GetPublicCollectionItemsAsync",
            "GetPublicSettingsAsync", "InvalidateSiteAsync"
        })
        {
            Assert.NotNull(contract.GetMethod(method));
        }
    }

    [Fact]
    public void PlacementAndCollectionItemKeys_AreScopedByGenerationAndParent()
    {
        var parentA = Guid.NewGuid();
        var parentB = Guid.NewGuid();
        Assert.NotEqual(ContentCacheKeys.PlacementItems("main", "a", parentA), ContentCacheKeys.PlacementItems("main", "b", parentA));
        Assert.NotEqual(ContentCacheKeys.PlacementItems("main", "a", parentA), ContentCacheKeys.PlacementItems("main", "a", parentB));
        Assert.NotEqual(ContentCacheKeys.CollectionItems("main", "a", parentA), ContentCacheKeys.CollectionItems("main", "a", parentB));
    }
}
