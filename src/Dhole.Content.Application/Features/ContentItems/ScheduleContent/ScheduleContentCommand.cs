using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;

namespace Dhole.Content.Application.ContentItems.ScheduleContent;

public sealed record ScheduleContentCommand(
    Guid Id,
    DateTime ScheduledAtUtc,
    DateTime? UnpublishAtUtc,
    Guid? ActorUserId) : ICommand<Result>;
