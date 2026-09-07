using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Content.Domain.Navigation.Entities;
namespace Dhole.Content.Application.Abstractions.Repositories;
public interface INavigationMenuRepository : IRepository<NavigationMenu,Guid>
{
    Task<NavigationMenu?> GetByLocationAsync(string siteKey,string location,CancellationToken cancellationToken=default);
}
