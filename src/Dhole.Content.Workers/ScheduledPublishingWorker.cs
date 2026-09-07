using CustomCodeFramework.Core.Abstractions;
using CustomCodeFramework.Workers.Abstractions;
using Dhole.Content.Application.Services;

namespace Dhole.Content.Workers;

internal sealed class ScheduledPublishingWorker(ContentApplicationService service, IDateTimeProvider clock, ILogger<ScheduledPublishingWorker> logger) : IBackgroundWorker
{
    public string Name => "content.scheduled-publishing";

    public async Task ExecuteAsync(IWorkerExecutionContext context, CancellationToken cancellationToken)
    {
        await service.PublishDueAsync(clock.UtcNow, cancellationToken);
        logger.LogDebug("Scheduled publishing background task completed at {ExecutedAtUtc}.", clock.UtcNow);
    }
}
