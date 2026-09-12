using CustomCodeFramework.Cqrs.Commands;

namespace Dhole.Content.Application.ContentItems.PublishDueContent;

public sealed record ScheduledContentProcessingResult(int PublishedCount, int UnpublishedCount);
public sealed record PublishDueContentCommand(int BatchSize = 100) : ICommand<ScheduledContentProcessingResult>;
