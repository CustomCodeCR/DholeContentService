using Dhole.Content.Contracts.Collections;
using Dhole.Content.Contracts.ContentItems;
using Dhole.Content.Contracts.Navigation;
using Dhole.Content.Contracts.Placements;
using Dhole.Content.Contracts.Settings;

namespace Dhole.Content.Application.Abstractions.Cache;

public interface IContentCacheService
{
    Task<ContentItemDto?> GetPublishedContentAsync(string siteKey, string slug, string locale, CancellationToken cancellationToken = default);
    Task SetPublishedContentAsync(string siteKey, string slug, string locale, ContentItemDto value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    Task RemoveContentAsync(string siteKey, string slug, CancellationToken cancellationToken = default);

    Task<ContentItemDto?> GetPublishedRouteAsync(string siteKey, string locale, string path, CancellationToken cancellationToken = default);
    Task SetPublishedRouteAsync(string siteKey, string locale, string path, ContentItemDto value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);

    Task<NavigationMenuDto?> GetMenuAsync(string siteKey, string location, CancellationToken cancellationToken = default);
    Task SetMenuAsync(string siteKey, string location, NavigationMenuDto value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    Task RemoveMenuAsync(string siteKey, string location, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<PlacementDto>?> GetPublicPlacementsAsync(string siteKey, CancellationToken cancellationToken = default);
    Task SetPublicPlacementsAsync(string siteKey, IReadOnlyCollection<PlacementDto> value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<PlacementItemDto>?> GetPublicPlacementItemsAsync(string siteKey, Guid placementId, CancellationToken cancellationToken = default);
    Task SetPublicPlacementItemsAsync(string siteKey, Guid placementId, IReadOnlyCollection<PlacementItemDto> value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<CollectionDto>?> GetPublicCollectionsAsync(string siteKey, CancellationToken cancellationToken = default);
    Task SetPublicCollectionsAsync(string siteKey, IReadOnlyCollection<CollectionDto> value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<CollectionItemDto>?> GetPublicCollectionItemsAsync(string siteKey, Guid collectionId, CancellationToken cancellationToken = default);
    Task SetPublicCollectionItemsAsync(string siteKey, Guid collectionId, IReadOnlyCollection<CollectionItemDto> value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<SiteSettingDto>?> GetPublicSettingsAsync(string siteKey, CancellationToken cancellationToken = default);
    Task SetPublicSettingsAsync(string siteKey, IReadOnlyCollection<SiteSettingDto> value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    Task RemovePublicSettingsAsync(string siteKey, CancellationToken cancellationToken = default);

    Task InvalidateSiteAsync(string siteKey, CancellationToken cancellationToken = default);
}
