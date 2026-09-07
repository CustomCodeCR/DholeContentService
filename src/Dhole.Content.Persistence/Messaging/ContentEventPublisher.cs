using System.Text.Json;
using CustomCodeFramework.Messaging.Outbox;
using Dhole.Content.Application.Abstractions;
using Dhole.Content.Domain.Content;
using Dhole.Content.Persistence.Auditing;
using Dhole.Content.Persistence.DbContexts;

namespace Dhole.Content.Persistence.Messaging;

public sealed class ContentEventPublisher(ServiceDbContext db) : IContentEventPublisher
{
    public Task QueueAsync(ContentItem item, string eventName, Guid? actorUserId, CancellationToken cancellationToken)
    {
        var correlationId = AuditExecutionContextAccessor.Current?.CorrelationId ?? Guid.NewGuid();
        db.OutboxMessages.Add(new OutboxMessage
        {
            EventId = Guid.NewGuid(),
            EventType = $"Dhole.Content.{ToEventType(eventName)}.v1",
            EventName = eventName,
            SourceService = "DholeContentService",
            PayloadJson = JsonSerializer.Serialize(new
            {
                item.Id,
                Type = item.Type.ToString(),
                Status = item.Status.ToString(),
                item.SiteKey,
                item.Slug,
                item.Title,
                item.ScheduledAtUtc,
                item.PublishedAtUtc,
                ActorUserId = actorUserId
            }),
            HeadersJson = null,
            CorrelationId = correlationId.ToString(),
            Status = OutboxMessageStatus.Pending,
            RetryCount = 0,
            ErrorMessage = null,
            CreatedAtUtc = DateTime.UtcNow
        });
        return Task.CompletedTask;
    }

    private static string ToEventType(string eventName)
        => string.Concat(eventName.Split('.', StringSplitOptions.RemoveEmptyEntries).Select(x => char.ToUpperInvariant(x[0]) + x[1..]));
}
