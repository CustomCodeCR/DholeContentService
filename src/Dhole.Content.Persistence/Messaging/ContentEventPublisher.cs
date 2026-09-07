using System.Text.Json;
using CustomCodeFramework.Messaging.Outbox;
using Dhole.Content.Application.Abstractions;
using Dhole.Content.Domain.Content;
using Dhole.Content.Persistence.DbContexts;

namespace Dhole.Content.Persistence.Messaging;

public sealed class ContentEventPublisher(ServiceDbContext db) : IContentEventPublisher
{
    public Task QueuePublishedAsync(ContentItem item, Guid? actorUserId, CancellationToken cancellationToken)
    {
        db.OutboxMessages.Add(new OutboxMessage
        {
            EventId = Guid.NewGuid(),
            EventType = "Dhole.Content.ContentPublished.v1",
            EventName = "content.published",
            SourceService = "DholeContentService",
            PayloadJson = JsonSerializer.Serialize(new
            {
                item.Id,
                Type = item.Type.ToString(),
                item.SiteKey,
                item.Slug,
                item.Title,
                item.PublishedAtUtc,
                ActorUserId = actorUserId
            }),
            HeadersJson = null,
            CorrelationId = Guid.NewGuid().ToString(),
            Status = OutboxMessageStatus.Pending,
            RetryCount = 0,
            ErrorMessage = null,
            CreatedAtUtc = DateTime.UtcNow
        });
        return db.SaveChangesAsync(cancellationToken);
    }
}
