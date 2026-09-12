using CustomCodeFramework.Core.Abstractions;
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Content.Application.Abstractions.Auditing;
using Dhole.Content.Application.Abstractions.Cache;
using Dhole.Content.Application.Abstractions.Repositories;
using Dhole.Content.Application.Auditing;

namespace Dhole.Content.Application.ContentItems.PublishDueContent;

public sealed class PublishDueContentCommandHandler(
    IContentItemRepository repo,
    IContentAuditService audit,
    IContentCacheService cache,
    IUnitOfWork uow,
    IDateTimeProvider clock) : ICommandHandler<PublishDueContentCommand, ScheduledContentProcessingResult>
{
    public async Task<ScheduledContentProcessingResult> HandleAsync(PublishDueContentCommand command, CancellationToken ct = default)
    {
        var utcNow = clock.UtcNow;
        var duePublish = await repo.GetDueScheduledAsync(utcNow, command.BatchSize, ct);
        var dueUnpublish = await repo.GetDueUnpublishAsync(utcNow, command.BatchSize, ct);

        foreach (var content in duePublish)
        {
            var before = ContentAuditSnapshots.From(content);
            content.Publish(utcNow, null);
            await audit.PublishAsync(new ContentAuditEvent(
                ContentAuditEventTypes.ContentPublished,
                ContentAuditActions.Published,
                ContentAuditEntityTypes.ContentItem,
                content.Id,
                ActorUserName: "Dhole.Content.Worker",
                Before: before,
                After: ContentAuditSnapshots.From(content),
                Metadata: new { Automatic = true, Worker = "ScheduledContentPublishingWorker", Trigger = "ScheduledAtUtc" }), ct);
        }

        foreach (var content in dueUnpublish)
        {
            var before = ContentAuditSnapshots.From(content);
            content.Unpublish(null);
            await audit.PublishAsync(new ContentAuditEvent(
                ContentAuditEventTypes.ContentUnpublished,
                ContentAuditActions.StatusChanged,
                ContentAuditEntityTypes.ContentItem,
                content.Id,
                ActorUserName: "Dhole.Content.Worker",
                Before: before,
                After: ContentAuditSnapshots.From(content),
                Metadata: new { Automatic = true, Worker = "ScheduledContentPublishingWorker", Trigger = "UnpublishAtUtc" }), ct);
        }

        if (duePublish.Count > 0 || dueUnpublish.Count > 0)
        {
            await uow.SaveChangesAsync(ct);

            foreach (var content in duePublish.Concat(dueUnpublish).GroupBy(x => new { x.SiteKey, x.Slug }).Select(x => x.First()))
                await cache.RemoveContentAsync(content.SiteKey, content.Slug, ct);
        }

        return new ScheduledContentProcessingResult(duePublish.Count, dueUnpublish.Count);
    }
}
