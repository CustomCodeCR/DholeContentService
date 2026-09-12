using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;

namespace Dhole.Content.Application.Routes.UpdateContentRoute;

public sealed record UpdateContentRouteCommand(
    Guid Id,
    string SiteKey,
    string Locale,
    string Path,
    bool IsPrimary,
    bool IsActive,
    Guid? ActorUserId,
    bool CreatePermanentRedirect = false
) : ICommand<Result>;
