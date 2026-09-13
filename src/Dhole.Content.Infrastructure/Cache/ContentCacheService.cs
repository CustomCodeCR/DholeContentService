using CustomCodeFramework.Redis.Abstractions;
using CustomCodeFramework.Redis.Caching;
using Dhole.Content.Application.Abstractions.Cache;
using Dhole.Content.Contracts.Collections;
using Dhole.Content.Contracts.ContentItems;
using Dhole.Content.Contracts.Navigation;
using Dhole.Content.Contracts.Placements;
using Dhole.Content.Contracts.Settings;

namespace Dhole.Content.Infrastructure.Cache;

public sealed class ContentCacheService(ICacheService cache) : IContentCacheService
{
    private const string InitialGeneration = "base";
    private static readonly TimeSpan DefaultExpiration = TimeSpan.FromMinutes(30);
    private static readonly TimeSpan GenerationExpiration = TimeSpan.FromDays(30);

    public async Task<ContentItemDto?> GetPublishedContentAsync(string siteKey, string slug, string locale, CancellationToken ct = default)
        => await cache.GetAsync<ContentItemDto>(ContentCacheKeys.PublishedContent(siteKey, await GetGenerationAsync(siteKey, ct), slug, locale), ct);

    public async Task SetPublishedContentAsync(string siteKey, string slug, string locale, ContentItemDto value, TimeSpan? expiration = null, CancellationToken ct = default)
        => await cache.SetAsync(ContentCacheKeys.PublishedContent(siteKey, await GetGenerationAsync(siteKey, ct), slug, locale), value, CacheEntryOptions.Default(expiration ?? DefaultExpiration), ct);

    public Task RemoveContentAsync(string siteKey, string slug, CancellationToken ct = default) => InvalidateSiteAsync(siteKey, ct);

    public async Task<ContentItemDto?> GetPublishedRouteAsync(string siteKey, string locale, string path, CancellationToken ct = default)
        => await cache.GetAsync<ContentItemDto>(ContentCacheKeys.PublishedRoute(siteKey, await GetGenerationAsync(siteKey, ct), locale, path), ct);

    public async Task SetPublishedRouteAsync(string siteKey, string locale, string path, ContentItemDto value, TimeSpan? expiration = null, CancellationToken ct = default)
        => await cache.SetAsync(ContentCacheKeys.PublishedRoute(siteKey, await GetGenerationAsync(siteKey, ct), locale, path), value, CacheEntryOptions.Default(expiration ?? DefaultExpiration), ct);

    public async Task<NavigationMenuDto?> GetMenuAsync(string siteKey, string location, CancellationToken ct = default)
        => await cache.GetAsync<NavigationMenuDto>(ContentCacheKeys.Menu(siteKey, await GetGenerationAsync(siteKey, ct), location), ct);

    public async Task SetMenuAsync(string siteKey, string location, NavigationMenuDto value, TimeSpan? expiration = null, CancellationToken ct = default)
        => await cache.SetAsync(ContentCacheKeys.Menu(siteKey, await GetGenerationAsync(siteKey, ct), location), value, CacheEntryOptions.Default(expiration ?? DefaultExpiration), ct);

    public Task RemoveMenuAsync(string siteKey, string location, CancellationToken ct = default) => InvalidateSiteAsync(siteKey, ct);

    public async Task<IReadOnlyCollection<PlacementDto>?> GetPublicPlacementsAsync(string siteKey, CancellationToken ct = default)
        => await cache.GetAsync<IReadOnlyCollection<PlacementDto>>(ContentCacheKeys.Placements(siteKey, await GetGenerationAsync(siteKey, ct)), ct);

    public async Task SetPublicPlacementsAsync(string siteKey, IReadOnlyCollection<PlacementDto> value, TimeSpan? expiration = null, CancellationToken ct = default)
        => await cache.SetAsync(ContentCacheKeys.Placements(siteKey, await GetGenerationAsync(siteKey, ct)), value, CacheEntryOptions.Default(expiration ?? DefaultExpiration), ct);

    public async Task<IReadOnlyCollection<PlacementItemDto>?> GetPublicPlacementItemsAsync(string siteKey, Guid placementId, CancellationToken ct = default)
        => await cache.GetAsync<IReadOnlyCollection<PlacementItemDto>>(ContentCacheKeys.PlacementItems(siteKey, await GetGenerationAsync(siteKey, ct), placementId), ct);

    public async Task SetPublicPlacementItemsAsync(string siteKey, Guid placementId, IReadOnlyCollection<PlacementItemDto> value, TimeSpan? expiration = null, CancellationToken ct = default)
        => await cache.SetAsync(ContentCacheKeys.PlacementItems(siteKey, await GetGenerationAsync(siteKey, ct), placementId), value, CacheEntryOptions.Default(expiration ?? DefaultExpiration), ct);

    public async Task<IReadOnlyCollection<CollectionDto>?> GetPublicCollectionsAsync(string siteKey, CancellationToken ct = default)
        => await cache.GetAsync<IReadOnlyCollection<CollectionDto>>(ContentCacheKeys.Collections(siteKey, await GetGenerationAsync(siteKey, ct)), ct);

    public async Task SetPublicCollectionsAsync(string siteKey, IReadOnlyCollection<CollectionDto> value, TimeSpan? expiration = null, CancellationToken ct = default)
        => await cache.SetAsync(ContentCacheKeys.Collections(siteKey, await GetGenerationAsync(siteKey, ct)), value, CacheEntryOptions.Default(expiration ?? DefaultExpiration), ct);

    public async Task<IReadOnlyCollection<CollectionItemDto>?> GetPublicCollectionItemsAsync(string siteKey, Guid collectionId, CancellationToken ct = default)
        => await cache.GetAsync<IReadOnlyCollection<CollectionItemDto>>(ContentCacheKeys.CollectionItems(siteKey, await GetGenerationAsync(siteKey, ct), collectionId), ct);

    public async Task SetPublicCollectionItemsAsync(string siteKey, Guid collectionId, IReadOnlyCollection<CollectionItemDto> value, TimeSpan? expiration = null, CancellationToken ct = default)
        => await cache.SetAsync(ContentCacheKeys.CollectionItems(siteKey, await GetGenerationAsync(siteKey, ct), collectionId), value, CacheEntryOptions.Default(expiration ?? DefaultExpiration), ct);

    public async Task<IReadOnlyCollection<SiteSettingDto>?> GetPublicSettingsAsync(string siteKey, CancellationToken ct = default)
        => await cache.GetAsync<IReadOnlyCollection<SiteSettingDto>>(ContentCacheKeys.Settings(siteKey, await GetGenerationAsync(siteKey, ct)), ct);

    public async Task SetPublicSettingsAsync(string siteKey, IReadOnlyCollection<SiteSettingDto> value, TimeSpan? expiration = null, CancellationToken ct = default)
        => await cache.SetAsync(ContentCacheKeys.Settings(siteKey, await GetGenerationAsync(siteKey, ct)), value, CacheEntryOptions.Default(expiration ?? DefaultExpiration), ct);

    public Task RemovePublicSettingsAsync(string siteKey, CancellationToken ct = default) => InvalidateSiteAsync(siteKey, ct);

    public Task InvalidateSiteAsync(string siteKey, CancellationToken ct = default)
        => cache.SetAsync(ContentCacheKeys.Generation(siteKey), Guid.NewGuid().ToString("N"), CacheEntryOptions.Default(GenerationExpiration), ct);

    private async Task<string> GetGenerationAsync(string siteKey, CancellationToken ct)
        => await cache.GetAsync<string>(ContentCacheKeys.Generation(siteKey), ct) ?? InitialGeneration;
}
