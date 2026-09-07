using CustomCodeFramework.Messaging.DependencyInjection;
using CustomCodeFramework.Messaging.Outbox.DependencyInjection;
using CustomCodeFramework.Mongo.DependencyInjection;
using CustomCodeFramework.Redis.DependencyInjection;
using CustomCodeFramework.Redis.Streams.DependencyInjection;
using CustomCodeFramework.Workers.DependencyInjection;
using Dhole.Content.Application.Abstractions;
using Dhole.Content.Application.Abstractions.Mongo;
using Dhole.Content.Infrastructure.Cache;
using Dhole.Content.Infrastructure.Mongo;
using Dhole.Content.Workers.Outbox;
using Dhole.Content.Workers.Streams;

namespace Dhole.Content.Workers.DependencyInjection;

public static class WorkerServiceCollectionExtensions
{
    public static IServiceCollection AddContentWorker(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCustomCodeRedis(configuration);
        services.AddCustomCodeMongo(configuration);
        services.AddCustomCodeRedisStreams(configuration);
        services.AddScoped<IContentCache, ContentCache>();
        services.AddScoped<IContentChangeSnapshotWriter, ContentChangeSnapshotWriter>();

        services.AddCustomCodeMessaging(configuration);
        services.AddCustomCodeMessagingOutbox(configuration);
        services.AddCustomCodeOutboxProcessor<OutboxProcessor>();
        services.AddCustomCodeInboxProcessor<InboxProcessor>();
        services.AddCustomCodeMessagingOutboxHostedServices();

        services.AddCustomCodeRedisStreamConsumerBackgroundService();
        services.AddCustomCodeRedisStreamHandler<ContentCreatedStreamHandler>();
        services.AddCustomCodeRedisStreamHandler<ContentUpdatedStreamHandler>();
        services.AddCustomCodeRedisStreamHandler<ContentPublishedStreamHandler>();
        services.AddCustomCodeRedisStreamHandler<ContentUnpublishedStreamHandler>();
        services.AddCustomCodeRedisStreamHandler<ContentScheduledStreamHandler>();
        services.AddCustomCodeRedisStreamHandler<ContentArchivedStreamHandler>();
        services.AddCustomCodeRedisStreamHandler<ContentDeletedStreamHandler>();

        services.AddCustomCodeWorkers(configuration);
        services.AddCustomCodePeriodicWorker<ScheduledPublishingWorker>();
        return services;
    }
}
