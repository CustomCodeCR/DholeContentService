using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Content.Contracts.Media;
using Dhole.Content.Domain.Media.Entities;

namespace Dhole.Content.Application.Abstractions.Repositories;

public interface IContentMediaRepository : IRepository<ContentMedia, Guid>
{
    Task<bool> ExistsAsync(
        Guid contentId,
        Guid mediaReferenceId,
        string role,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<ContentMediaDto>> GetByContentAsync(
        Guid contentId,
        string? role = null,
        CancellationToken cancellationToken = default);
}
