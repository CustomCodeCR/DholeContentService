using Dhole.Content.Contracts.ContentItems;
using Dhole.Content.Contracts.Navigation;
using Dhole.Content.Contracts.Settings;
namespace Dhole.Content.Application.Abstractions.Cache;
public interface IContentCacheService
{
    Task<ContentItemDto?> GetPublishedContentAsync(string siteKey,string slug,string locale,CancellationToken cancellationToken=default);
    Task SetPublishedContentAsync(string siteKey,string slug,string locale,ContentItemDto value,TimeSpan? expiration=null,CancellationToken cancellationToken=default);
    Task RemoveContentAsync(string siteKey,string slug,CancellationToken cancellationToken=default);
    Task<NavigationMenuDto?> GetMenuAsync(string siteKey,string location,CancellationToken cancellationToken=default);
    Task SetMenuAsync(string siteKey,string location,NavigationMenuDto value,TimeSpan? expiration=null,CancellationToken cancellationToken=default);
    Task RemoveMenuAsync(string siteKey,string location,CancellationToken cancellationToken=default);
    Task<IReadOnlyCollection<SiteSettingDto>?> GetPublicSettingsAsync(string siteKey,CancellationToken cancellationToken=default);
    Task SetPublicSettingsAsync(string siteKey,IReadOnlyCollection<SiteSettingDto> value,TimeSpan? expiration=null,CancellationToken cancellationToken=default);
    Task RemovePublicSettingsAsync(string siteKey,CancellationToken cancellationToken=default);
}
