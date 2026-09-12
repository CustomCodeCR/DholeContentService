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
using Dhole.Content.Domain.Shared;

namespace Dhole.Content.Application.ContentItems.PublishContent;

public sealed class PublishContentCommandHandler(
    IContentItemRepository repo,
    IContentReviewRepository reviews,
    IContentAuditService audit,
    IContentCacheService cache,
    IUnitOfWork uow,
    IDateTimeProvider clock) : ICommandHandler<PublishContentCommand, Result>
{
    public async Task<Result> HandleAsync(PublishContentCommand command, CancellationToken ct = default)
    {
        var content = await repo.GetByIdWithDetailsAsync(command.Id, ct);
        if (content is null || content.IsDeleted) return Result.Failure(ContentErrors.ContentNotFound);
        if (content.Status is ContentStatus.Published or ContentStatus.Archived)
            return Result.Failure(ContentErrors.InvalidContentState);

        var before = ContentAuditSnapshots.From(content);
        ContentReview? review = null;

        if (content.Status != ContentStatus.Scheduled)
        {
            review = await reviews.GetPendingByContentAsync(content.Id, ct);
            if (review is not null && content.Status != ContentStatus.PendingReview)
                return Result.Failure(ContentErrors.InvalidContentState);

            if (review is null)
            {
                if (content.Status is not ContentStatus.Draft and not ContentStatus.Rejected and not ContentStatus.PendingReview)
                    return Result.Failure(ContentErrors.InvalidContentState);

                var reason = content.Status == ContentStatus.PendingReview ? "legacy-pending-review" : "submitted-for-review";
                var revision = content.CreateRevision(command.ActorUserId, reason);
                review = ContentReview.Submit(content.Id, revision.Id, command.ActorUserId, clock.UtcNow);
                await reviews.AddAsync(review, ct);

                if (content.Status != ContentStatus.PendingReview)
                {
                    content.SubmitForReview(command.ActorUserId);
                    await audit.PublishAsync(new ContentAuditEvent(
                        ContentAuditEventTypes.ContentReviewRequested,
                        ContentAuditActions.StatusChanged,
                        ContentAuditEntityTypes.ContentItem,
                        content.Id,
                        command.ActorUserId,
                        Before: before,
                        After: ContentAuditSnapshots.From(content)), ct);
                }

                await audit.PublishAsync(new ContentAuditEvent(
                    ContentAuditEventTypes.ContentReviewSubmitted,
                    ContentAuditActions.StatusChanged,
                    ContentAuditEntityTypes.ContentReview,
                    review.Id,
                    command.ActorUserId,
                    After: ApproveContentReviewCommandHandler.ReviewSnapshot(review)), ct);
            }

            try
            {
                review.Approve(command.ActorUserId, null, clock.UtcNow);
            }
            catch (InvalidOperationException)
            {
                return Result.Failure(ContentErrors.ContentReviewNotPending);
            }

            await audit.PublishAsync(new ContentAuditEvent(
                ContentAuditEventTypes.ContentReviewApproved,
                ContentAuditActions.StatusChanged,
                ContentAuditEntityTypes.ContentReview,
                review.Id,
                command.ActorUserId,
                After: ApproveContentReviewCommandHandler.ReviewSnapshot(review)), ct);
        }

        try
        {
            content.Publish(clock.UtcNow, command.ActorUserId);
        }
        catch (InvalidOperationException)
        {
            return Result.Failure(ContentErrors.InvalidContentState);
        }

        await audit.PublishAsync(new ContentAuditEvent(
            ContentAuditEventTypes.ContentPublished,
            ContentAuditActions.Published,
            ContentAuditEntityTypes.ContentItem,
            content.Id,
            command.ActorUserId,
            Before: before,
            After: ContentAuditSnapshots.From(content)), ct);
        await uow.SaveChangesAsync(ct);
        await cache.RemoveContentAsync(content.SiteKey, content.Slug, ct);
        return Result.Success();
    }
}
