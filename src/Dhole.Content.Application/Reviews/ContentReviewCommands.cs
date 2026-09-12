using CustomCodeFramework.Core.Abstractions;
using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Content.Application.Abstractions.Auditing;
using Dhole.Content.Application.Abstractions.Cache;
using Dhole.Content.Application.Abstractions.Repositories;
using Dhole.Content.Application.Auditing;
using Dhole.Content.Domain.ContentItems.Enums;
using Dhole.Content.Domain.Reviews.Entities;
using Dhole.Content.Domain.Reviews.Enums;
using Dhole.Content.Domain.Shared;

namespace Dhole.Content.Application.Reviews;

public sealed record ApproveContentReviewCommand(Guid ReviewId, string? Comment, Guid? ActorUserId) : ICommand<Result>;
public sealed record RejectContentReviewCommand(Guid ReviewId, string? Comment, Guid? ActorUserId) : ICommand<Result>;

public sealed class ApproveContentReviewCommandHandler(
    IContentReviewRepository reviews,
    IContentItemRepository contents,
    IContentAuditService audit,
    IContentCacheService cache,
    IUnitOfWork unitOfWork,
    IDateTimeProvider clock) : ICommandHandler<ApproveContentReviewCommand, Result>
{
    public async Task<Result> HandleAsync(ApproveContentReviewCommand command, CancellationToken cancellationToken = default)
    {
        var review = await reviews.GetByIdAsync(command.ReviewId, cancellationToken);
        if (review is null || review.IsDeleted) return Result.Failure(ContentErrors.ContentReviewNotFound);
        if (review.Status != ContentReviewStatus.Pending) return Result.Failure(ContentErrors.ContentReviewNotPending);

        var content = await contents.GetByIdWithDetailsAsync(review.ContentId, cancellationToken);
        if (content is null || content.IsDeleted) return Result.Failure(ContentErrors.ContentNotFound);
        if (content.Status != ContentStatus.PendingReview) return Result.Failure(ContentErrors.InvalidContentState);

        var before = ContentAuditSnapshots.From(content);
        try
        {
            review.Approve(command.ActorUserId, command.Comment, clock.UtcNow);
            content.Publish(clock.UtcNow, command.ActorUserId);
        }
        catch (ArgumentException)
        {
            return Result.Failure(ContentErrors.InvalidContentReviewData);
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
            After: ReviewSnapshot(review)), cancellationToken);
        await audit.PublishAsync(new ContentAuditEvent(
            ContentAuditEventTypes.ContentPublished,
            ContentAuditActions.Published,
            ContentAuditEntityTypes.ContentItem,
            content.Id,
            command.ActorUserId,
            Before: before,
            After: ContentAuditSnapshots.From(content)), cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await cache.RemoveContentAsync(content.SiteKey, content.Slug, cancellationToken);
        return Result.Success();
    }

    internal static object ReviewSnapshot(ContentReview review) => new
    {
        review.Id,
        review.ContentId,
        review.RevisionId,
        review.SubmittedByUserId,
        review.ReviewerUserId,
        Status = review.Status.ToString(),
        review.Comment,
        review.SubmittedAtUtc,
        review.DecidedAtUtc
    };
}

public sealed class RejectContentReviewCommandHandler(
    IContentReviewRepository reviews,
    IContentItemRepository contents,
    IContentAuditService audit,
    IContentCacheService cache,
    IUnitOfWork unitOfWork,
    IDateTimeProvider clock) : ICommandHandler<RejectContentReviewCommand, Result>
{
    public async Task<Result> HandleAsync(RejectContentReviewCommand command, CancellationToken cancellationToken = default)
    {
        var review = await reviews.GetByIdAsync(command.ReviewId, cancellationToken);
        if (review is null || review.IsDeleted) return Result.Failure(ContentErrors.ContentReviewNotFound);
        if (review.Status != ContentReviewStatus.Pending) return Result.Failure(ContentErrors.ContentReviewNotPending);

        var content = await contents.GetByIdWithDetailsAsync(review.ContentId, cancellationToken);
        if (content is null || content.IsDeleted) return Result.Failure(ContentErrors.ContentNotFound);
        if (content.Status != ContentStatus.PendingReview) return Result.Failure(ContentErrors.InvalidContentState);

        var before = ContentAuditSnapshots.From(content);
        try
        {
            review.Reject(command.ActorUserId, command.Comment, clock.UtcNow);
            content.RejectReview(command.ActorUserId);
        }
        catch (ArgumentException)
        {
            return Result.Failure(ContentErrors.InvalidContentReviewData);
        }
        catch (InvalidOperationException)
        {
            return Result.Failure(ContentErrors.ContentReviewNotPending);
        }

        await audit.PublishAsync(new ContentAuditEvent(
            ContentAuditEventTypes.ContentReviewRejected,
            ContentAuditActions.StatusChanged,
            ContentAuditEntityTypes.ContentReview,
            review.Id,
            command.ActorUserId,
            After: ApproveContentReviewCommandHandler.ReviewSnapshot(review)), cancellationToken);
        await audit.PublishAsync(new ContentAuditEvent(
            ContentAuditEventTypes.ContentReviewRejected,
            ContentAuditActions.StatusChanged,
            ContentAuditEntityTypes.ContentItem,
            content.Id,
            command.ActorUserId,
            Before: before,
            After: ContentAuditSnapshots.From(content)), cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await cache.RemoveContentAsync(content.SiteKey, content.Slug, cancellationToken);
        return Result.Success();
    }
}
