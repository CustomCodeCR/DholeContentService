using CustomCodeFramework.Core.Abstractions;
using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Content.Application.Abstractions.Auditing;
using Dhole.Content.Application.Abstractions.Cache;
using Dhole.Content.Application.Abstractions.Repositories;
using Dhole.Content.Application.Auditing;
using Dhole.Content.Application.Reviews;
using Dhole.Content.Domain.ContentItems.Enums;
using Dhole.Content.Domain.Reviews.Entities;
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
        if (content.Status is ContentStatus.Published or ContentStatus.Archived or ContentStatus.Scheduled)
            return Result.Failure(ContentErrors.InvalidContentState);
        if (c.ScheduledAtUtc <= clock.UtcNow) return Result.Failure(ContentErrors.InvalidScheduleDate);
        if (c.UnpublishAtUtc.HasValue && c.UnpublishAtUtc.Value <= c.ScheduledAtUtc)
            return Result.Failure(ContentErrors.InvalidPublicationWindow);

        var contentBefore = ContentAuditSnapshots.From(content);
        var review = await reviews.GetPendingByContentAsync(content.Id, ct);

        if (review is not null && content.Status != ContentStatus.PendingReview)
            return Result.Failure(ContentErrors.InvalidContentState);

        if (review is null)
        {
            if (content.Status is not ContentStatus.Draft and not ContentStatus.Rejected and not ContentStatus.PendingReview)
                return Result.Failure(ContentErrors.InvalidContentState);

            var reason = content.Status == ContentStatus.PendingReview
                ? "legacy-pending-review"
                : "submitted-for-review";
            var revision = content.CreateRevision(c.ActorUserId, reason);
            review = ContentReview.Submit(content.Id, revision.Id, c.ActorUserId, clock.UtcNow);
            await reviews.AddAsync(review, ct);

            if (content.Status != ContentStatus.PendingReview)
            {
                content.SubmitForReview(c.ActorUserId);
                await audit.PublishAsync(new ContentAuditEvent(
                    ContentAuditEventTypes.ContentReviewRequested,
                    ContentAuditActions.StatusChanged,
                    ContentAuditEntityTypes.ContentItem,
                    content.Id,
                    c.ActorUserId,
                    Before: contentBefore,
                    After: ContentAuditSnapshots.From(content)), ct);
            }

            await audit.PublishAsync(new ContentAuditEvent(
                ContentAuditEventTypes.ContentReviewSubmitted,
                ContentAuditActions.StatusChanged,
                ContentAuditEntityTypes.ContentReview,
                review.Id,
                c.ActorUserId,
                After: ApproveContentReviewCommandHandler.ReviewSnapshot(review)), ct);
        }

        if (review.IsDeleted || review.Status != ContentReviewStatus.Pending)
            return Result.Failure(ContentErrors.ContentReviewNotPending);

        var reviewBefore = ApproveContentReviewCommandHandler.ReviewSnapshot(review);
        try
        {
            review.Approve(c.ActorUserId, null, clock.UtcNow);
            content.Schedule(c.ScheduledAtUtc, c.UnpublishAtUtc, clock.UtcNow, c.ActorUserId);
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
            ContentAuditActions.Approved,
            ContentAuditEntityTypes.ContentReview,
            review.Id,
            c.ActorUserId,
            Before: reviewBefore,
            After: ApproveContentReviewCommandHandler.ReviewSnapshot(review),
            Metadata: new { ScheduledPublication = true }), ct);

        await audit.PublishAsync(new ContentAuditEvent(
            ContentAuditEventTypes.ContentScheduled,
            ContentAuditActions.Scheduled,
            ContentAuditEntityTypes.ContentItem,
            content.Id,
            c.ActorUserId,
            Before: contentBefore,
            After: ContentAuditSnapshots.From(content),
            Metadata: new { c.ScheduledAtUtc, c.UnpublishAtUtc }), ct);

        await uow.SaveChangesAsync(ct);
        await cache.RemoveContentAsync(content.SiteKey, content.Slug, ct);
        return Result.Success();
    }
}
