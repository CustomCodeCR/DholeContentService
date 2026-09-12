using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Content.Domain.Routes.Entities;

namespace Dhole.Content.Application.Abstractions.Repositories;

public interface IContentRouteRepository : IRepository<ContentRoute, Guid>
{
    Task<ContentRoute?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ContentRoute?> GetByPathAsync(string siteKey, string locale, string path, bool activeOnly = true, CancellationToken cancellationToken = default);
    Task<ContentRoute?> GetPrimaryAsync(Guid contentId, string locale, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<ContentRoute>> GetByContentAsync(Guid contentId, CancellationToken cancellationToken = default);
    Task<bool> ExistsPathAsync(string siteKey, string locale, string path, Guid? excludeId = null, CancellationToken cancellationToken = default);
}
