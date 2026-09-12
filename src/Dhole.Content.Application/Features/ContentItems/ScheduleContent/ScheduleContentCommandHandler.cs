using CustomCodeFramework.Core.Abstractions;
using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Content.Application.Abstractions.Auditing;
using Dhole.Content.Application.Abstractions.Cache;
using Dhole.Content.Application.Abstractions.Repositories;
using Dhole.Content.Application.Auditing;
using Dhole.Content.Domain.ContentItems.Enums;
using Dhole.Content.Domain.Reviews.Enums;
using Dhole.Content.Domain.Shared;

namespace Dhole.Content.Application.ContentItems.ScheduleContent;

public sealed class ScheduleContentCommandHandler(
    IContentItemRepository repo,
    IContentReviewRepository reviews,
    IContentAuditService audit,
    IContentCacheService cache,
    IUnitOfWork uow,
    IDateTimeProvider clock) : ICommandHandler<ScheduleContentCommand, Result>
{
    public async Task<Result> HandleAsync(ScheduleContentCommand c, CancellationToken ct = default)
    {
        var content = await repo.GetByIdWithDetailsAsync(c.Id, ct);
        if (content is null || content.IsDeleted) return Result.Failure(ContentErrors.ContentNotFound);
        if (content.Status != ContentStatus.PendingReview) return Result.Failure(ContentErrors.InvalidContentState);
        if (c.ScheduledAtUtc <= clock.UtcNow) return Result.Failure(ContentErrors.InvalidScheduleDate);
        if (c.UnpublishAtUtc.HasValue && c.UnpublishAtUtc.Value <= c.ScheduledAtUtc)
            return Result.Failure(ContentErrors.InvalidPublicationWindow);

        var review = await reviews.GetPendingByContentAsync(content.Id, ct);
        if (review is null || review.IsDeleted || review.Status != ContentReviewStatus.Pending)
            return Result.Failure(ContentErrors.ContentReviewNotFound);

        var before = ContentAuditSnapshots.From(content);
        try
        {
            content.Schedule(c.ScheduledAtUtc, c.UnpublishAtUtc, clock.UtcNow, c.ActorUserId);
            review.Approve(c.ActorUserId, null, clock.UtcNow);
        }
        catch (ArgumentException)
        {
            return Result.Failure(ContentErrors.InvalidPublicationWindow);
        }
        catch (InvalidOperationException)
        {
            return Result.Failure(ContentErrors.InvalidContentState);
        }

        await audit.PublishAsync(new ContentAuditEvent(
            ContentAuditEventTypes.ContentReviewApproved,
            ContentAuditActions.StatusChanged,
            ContentAuditEntityTypes.ContentReview,
            review.Id,
            c.ActorUserId,
            After: new
            {
                review.Id,
                review.ContentId,
                review.RevisionId,
                review.SubmittedByUserId,
                review.ReviewerUserId,
                Status = review.Status.ToString(),
                review.Comment,
                review.SubmittedAtUtc,
                review.DecidedAtUtc,
                ScheduledPublication = true
            }), ct);

        await audit.PublishAsync(new ContentAuditEvent(
            ContentAuditEventTypes.ContentScheduled,
            ContentAuditActions.Scheduled,
            ContentAuditEntityTypes.ContentItem,
            content.Id,
            c.ActorUserId,
            Before: before,
            After: ContentAuditSnapshots.From(content),
            Metadata: new { c.ScheduledAtUtc, c.UnpublishAtUtc }), ct);

        await uow.SaveChangesAsync(ct);
        await cache.RemoveContentAsync(content.SiteKey, content.Slug, ct);
        return Result.Success();
    }
}
