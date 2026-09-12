using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Content.Domain.Redirects.Entities;

namespace Dhole.Content.Application.Abstractions.Repositories;

public interface IRedirectRepository : IRepository<ContentRedirect, Guid>
{
    Task<ContentRedirect?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ContentRedirect?> GetBySourcePathAsync(string siteKey, string sourcePath, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<ContentRedirect>> GetAllAsync(string? siteKey = null, CancellationToken cancellationToken = default);
    Task<bool> ExistsSourcePathAsync(string siteKey, string sourcePath, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<ContentRedirect?> ResolveAsync(string siteKey, string sourcePath, DateTime utcNow, CancellationToken cancellationToken = default);
}
