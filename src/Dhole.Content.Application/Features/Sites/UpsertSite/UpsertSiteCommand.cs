using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;

namespace Dhole.Content.Application.Sites.UpsertSite;

public sealed record UpsertSiteCommand(
    string SiteKey,
    string Name,
    string PrimaryDomain,
    string DefaultLocale,
    string TimeZone,
    Guid? LogoMediaId,
    Guid? FaviconMediaId,
    Guid? DefaultOpenGraphMediaId,
    string? Status,
    Guid? ActorUserId
) : ICommand<Result<Guid>>;
