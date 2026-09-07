using Dhole.Content.Application.Abstractions;
using Dhole.Content.Application.Abstractions.Mongo;

namespace Dhole.Content.Workers.Streams;

internal sealed class ContentCreatedStreamHandler(IContentCache cache, IContentChangeSnapshotWriter snapshots, ILogger<ContentCreatedStreamHandler> logger) : ContentCacheInvalidationStreamHandlerBase(cache, snapshots, logger) { public override string MessageType => "content.created"; }
internal sealed class ContentUpdatedStreamHandler(IContentCache cache, IContentChangeSnapshotWriter snapshots, ILogger<ContentUpdatedStreamHandler> logger) : ContentCacheInvalidationStreamHandlerBase(cache, snapshots, logger) { public override string MessageType => "content.updated"; }
internal sealed class ContentPublishedStreamHandler(IContentCache cache, IContentChangeSnapshotWriter snapshots, ILogger<ContentPublishedStreamHandler> logger) : ContentCacheInvalidationStreamHandlerBase(cache, snapshots, logger) { public override string MessageType => "content.published"; }
internal sealed class ContentUnpublishedStreamHandler(IContentCache cache, IContentChangeSnapshotWriter snapshots, ILogger<ContentUnpublishedStreamHandler> logger) : ContentCacheInvalidationStreamHandlerBase(cache, snapshots, logger) { public override string MessageType => "content.unpublished"; }
internal sealed class ContentScheduledStreamHandler(IContentCache cache, IContentChangeSnapshotWriter snapshots, ILogger<ContentScheduledStreamHandler> logger) : ContentCacheInvalidationStreamHandlerBase(cache, snapshots, logger) { public override string MessageType => "content.scheduled"; }
internal sealed class ContentArchivedStreamHandler(IContentCache cache, IContentChangeSnapshotWriter snapshots, ILogger<ContentArchivedStreamHandler> logger) : ContentCacheInvalidationStreamHandlerBase(cache, snapshots, logger) { public override string MessageType => "content.archived"; }
internal sealed class ContentDeletedStreamHandler(IContentCache cache, IContentChangeSnapshotWriter snapshots, ILogger<ContentDeletedStreamHandler> logger) : ContentCacheInvalidationStreamHandlerBase(cache, snapshots, logger) { public override string MessageType => "content.deleted"; }
