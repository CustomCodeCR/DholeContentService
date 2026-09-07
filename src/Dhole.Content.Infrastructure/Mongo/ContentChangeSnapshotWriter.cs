using CustomCodeFramework.Mongo.Abstractions;
using Dhole.Content.Application.Abstractions.Mongo;
using Dhole.Content.Infrastructure.Mongo.Documents;

namespace Dhole.Content.Infrastructure.Mongo;

public sealed class ContentChangeSnapshotWriter(IMongoContext mongoContext) : IContentChangeSnapshotWriter
{
    public Task WriteAsync(string eventId, string eventName, string? entityId, string? siteKey, string? slug, string payloadJson, string? correlationId, DateTime changedAtUtc, CancellationToken cancellationToken = default)
    {
        var document = new ContentChangeSnapshotDocument
        {
            EventId = eventId,
            EventName = eventName,
            EntityId = entityId,
            SiteKey = siteKey,
            Slug = slug,
            PayloadJson = payloadJson,
            CorrelationId = correlationId,
            ChangedAtUtc = changedAtUtc
        };
        return mongoContext.GetCollection<ContentChangeSnapshotDocument>().InsertOneAsync(document, cancellationToken: cancellationToken);
    }
}
