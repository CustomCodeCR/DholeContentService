using CustomCodeFramework.Postgres.EntityFramework.Repositories;
using Dhole.Content.Application.Abstractions.Repositories;
using Dhole.Content.Domain.Reviews.Entities;
using Dhole.Content.Domain.Reviews.Enums;
using Dhole.Content.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Dhole.Content.Persistence.Repositories;

public sealed class ContentReviewRepository(ServiceDbContext db)
    : EfRepository<ContentReview, Guid>(db), IContentReviewRepository
{
    public Task<ContentReview?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => db.ContentReviews.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

    public Task<ContentReview?> GetPendingByContentAsync(Guid contentId, CancellationToken cancellationToken = default)
        => db.ContentReviews.FirstOrDefaultAsync(x =>
            x.ContentId == contentId && !x.IsDeleted && x.Status == ContentReviewStatus.Pending,
            cancellationToken);

    public async Task<IReadOnlyCollection<ContentReview>> GetAllAsync(
        Guid? contentId = null,
        ContentReviewStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = db.ContentReviews.AsNoTracking().Where(x => !x.IsDeleted);
        if (contentId.HasValue) query = query.Where(x => x.ContentId == contentId.Value);
        if (status.HasValue) query = query.Where(x => x.Status == status.Value);
        return await query.OrderByDescending(x => x.SubmittedAtUtc).ToListAsync(cancellationToken);
    }
}
