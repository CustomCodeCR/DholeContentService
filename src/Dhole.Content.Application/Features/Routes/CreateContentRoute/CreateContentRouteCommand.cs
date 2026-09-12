using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;

namespace Dhole.Content.Application.Routes.CreateContentRoute;

public sealed record CreateContentRouteCommand(
    string SiteKey,
    Guid ContentId,
    string Locale,
    string Path,
    bool IsPrimary,
    bool IsActive,
    Guid? ActorUserId
) : ICommand<Result<Guid>>;
