using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;

namespace Dhole.Content.Application.Media.RemoveContentMedia;

public sealed record RemoveContentMediaCommand(Guid ContentId, Guid Id, Guid? ActorUserId) : ICommand<Result>;
