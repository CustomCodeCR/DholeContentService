using CustomCodeFramework.Mongo.Abstractions;
using CustomCodeFramework.Mongo.Collections;

namespace Dhole.Content.Infrastructure.Mongo.Documents;

[MongoCollectionName("content_change_snapshots")]
public sealed class ContentChangeSnapshotDocument : IReadModel
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public string EventId { get; init; } = default!;
    public string EventName { get; init; } = default!;
    public string? EntityId { get; init; }
    public string? SiteKey { get; init; }
    public string? Slug { get; init; }
    public string PayloadJson { get; init; } = default!;
    public string? CorrelationId { get; init; }
    public DateTime ChangedAtUtc { get; init; }
    public string SourceService { get; init; } = "DholeContentService";
}
