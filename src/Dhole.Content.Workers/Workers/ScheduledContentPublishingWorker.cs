using CustomCodeFramework.Core.Abstractions;
using CustomCodeFramework.Cqrs.Dispatching;
using CustomCodeFramework.Workers.Abstractions;
using Dhole.Content.Application.ContentItems.PublishDueContent;

namespace Dhole.Content.Worker.Workers;

internal sealed class ScheduledContentPublishingWorker(
    ICommandDispatcher dispatcher,
    IDateTimeProvider clock,
    ILogger<ScheduledContentPublishingWorker> logger) : IBackgroundWorker
{
    public string Name => "content.scheduled-publishing";

    public async Task ExecuteAsync(IWorkerExecutionContext context, CancellationToken ct)
    {
        var result = await dispatcher.DispatchAsync(new PublishDueContentCommand(), ct);
        logger.LogInformation(
            "Scheduled content processing completed. Published={PublishedCount}, Unpublished={UnpublishedCount}, ExecutedAtUtc={ExecutedAtUtc}.",
            result.PublishedCount,
            result.UnpublishedCount,
            clock.UtcNow);
    }
}
