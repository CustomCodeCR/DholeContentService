using System.Text.Json;
using CustomCodeFramework.Redis.Streams.Abstractions;
using CustomCodeFramework.Redis.Streams.Messages;
using Dhole.Content.Application.Abstractions;
using Dhole.Content.Application.Abstractions.Mongo;

namespace Dhole.Content.Workers.Streams;

internal abstract class ContentCacheInvalidationStreamHandlerBase(IContentCache cache, IContentChangeSnapshotWriter snapshots, ILogger logger) : IRedisStreamMessageHandler
{
    public abstract string MessageType { get; }

    public async Task HandleAsync(RedisStreamEnvelope envelope, CancellationToken cancellationToken = default)
    {
        var identity = ReadIdentity(envelope.PayloadJson);
        if (!string.IsNullOrWhiteSpace(identity.Slug))
            await cache.RemoveContentAsync(string.IsNullOrWhiteSpace(identity.SiteKey) ? "main" : identity.SiteKey, identity.Slug, cancellationToken);

        await snapshots.WriteAsync(
            envelope.MessageId,
            envelope.MessageType,
            identity.EntityId,
            identity.SiteKey,
            identity.Slug,
            envelope.PayloadJson ?? "{}",
            null,
            DateTime.UtcNow,
            cancellationToken);

        logger.LogInformation("Content event {MessageType} projected to Mongo/cache. MessageId: {MessageId}, SiteKey: {SiteKey}, Slug: {Slug}.", envelope.MessageType, envelope.MessageId, identity.SiteKey, identity.Slug);
    }

    private static (string? EntityId, string? SiteKey, string? Slug) ReadIdentity(string? payloadJson)
    {
        if (string.IsNullOrWhiteSpace(payloadJson)) return (null, null, null);
        using var document = JsonDocument.Parse(payloadJson);
        var root = document.RootElement;
        return (Read(root, "id") ?? Read(root, "Id"), Read(root, "siteKey") ?? Read(root, "SiteKey"), Read(root, "slug") ?? Read(root, "Slug"));
    }

    private static string? Read(JsonElement element, string name)
        => element.TryGetProperty(name, out var property) ? property.ToString() : null;
}
