using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Content.Domain.Reviews.Entities;
using Dhole.Content.Domain.Reviews.Enums;

namespace Dhole.Content.Application.Abstractions.Repositories;

public interface IContentReviewRepository : IRepository<ContentReview, Guid>
{
    Task<ContentReview?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ContentReview?> GetPendingByContentAsync(Guid contentId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<ContentReview>> GetAllAsync(Guid? contentId = null, ContentReviewStatus? status = null, CancellationToken cancellationToken = default);
}
