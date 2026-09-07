namespace Dhole.Content.Application.Abstractions.Mongo;

public interface IContentChangeSnapshotWriter
{
    Task WriteAsync(
        string eventId,
        string eventName,
        string? entityId,
        string? siteKey,
        string? slug,
        string payloadJson,
        string? correlationId,
        DateTime changedAtUtc,
        CancellationToken cancellationToken = default);
}
