using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Content.Domain.Settings.Entities;
namespace Dhole.Content.Application.Abstractions.Repositories;
public interface ISiteSettingRepository : IRepository<SiteSetting,Guid>
{
    Task<SiteSetting?> GetByKeyAsync(string siteKey,string key,CancellationToken cancellationToken=default);
    Task<IReadOnlyCollection<SiteSetting>> GetBySiteAsync(string siteKey,bool publicOnly=false,CancellationToken cancellationToken=default);
}
