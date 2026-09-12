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

namespace Dhole.Content.Application.ContentItems.SubmitContentForReview;

public sealed class SubmitContentForReviewCommandHandler(
    IContentItemRepository repo,
    IContentReviewRepository reviews,
    IContentAuditService audit,
    IContentCacheService cache,
    IUnitOfWork uow,
    IDateTimeProvider clock) : ICommandHandler<SubmitContentForReviewCommand, Result>
{
    public async Task<Result> HandleAsync(SubmitContentForReviewCommand command, CancellationToken ct = default)
    {
        var content = await repo.GetByIdWithDetailsAsync(command.Id, ct);
        if (content is null || content.IsDeleted) return Result.Failure(ContentErrors.ContentNotFound);
        if (content.Status is not ContentStatus.Draft and not ContentStatus.Rejected)
            return Result.Failure(ContentErrors.InvalidContentState);
        if (await reviews.GetPendingByContentAsync(content.Id, ct) is not null)
            return Result.Failure(ContentErrors.ContentReviewAlreadyPending);

        var before = ContentAuditSnapshots.From(content);
        var revision = content.CreateRevision(command.ActorUserId, "submitted-for-review");
        var review = ContentReview.Submit(content.Id, revision.Id, command.ActorUserId, clock.UtcNow);
        await reviews.AddAsync(review, ct);

        try
        {
            content.SubmitForReview(command.ActorUserId);
        }
        catch (InvalidOperationException)
        {
            return Result.Failure(ContentErrors.InvalidContentState);
        }

        await audit.PublishAsync(new ContentAuditEvent(
            ContentAuditEventTypes.ContentReviewRequested,
            ContentAuditActions.StatusChanged,
            ContentAuditEntityTypes.ContentItem,
            content.Id,
            command.ActorUserId,
            Before: before,
            After: ContentAuditSnapshots.From(content)), ct);
        await audit.PublishAsync(new ContentAuditEvent(
            ContentAuditEventTypes.ContentReviewSubmitted,
            ContentAuditActions.StatusChanged,
            ContentAuditEntityTypes.ContentReview,
            review.Id,
            command.ActorUserId,
            After: ApproveContentReviewCommandHandler.ReviewSnapshot(review)), ct);

        await uow.SaveChangesAsync(ct);
        await cache.RemoveContentAsync(content.SiteKey, content.Slug, ct);
        return Result.Success();
    }
}
