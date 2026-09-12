using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Content.Application.Abstractions.Repositories;
using Dhole.Content.Domain.Routes.Entities;
using Dhole.Content.Domain.Shared;

namespace Dhole.Content.Application.Routes.CreateContentRoute;

public sealed class CreateContentRouteCommandHandler(
    IContentRouteRepository routes,
    IContentItemRepository contents,
    ISiteRepository sites,
    IUnitOfWork unitOfWork
) : ICommandHandler<CreateContentRouteCommand, Result<Guid>>
{
    public async Task<Result<Guid>> HandleAsync(CreateContentRouteCommand command, CancellationToken ct = default)
    {
        var siteKey = command.SiteKey.Trim().ToLowerInvariant();
        var locale = command.Locale.Trim();
        var path = ContentRoute.NormalizePath(command.Path);

        var site = await sites.GetBySiteKeyAsync(siteKey, ct);
        if (site is null || site.IsDeleted)
            return Result.Failure<Guid>(ContentErrors.SiteNotFound);

        var content = await contents.GetByIdWithDetailsAsync(command.ContentId, ct);
        if (content is null || content.IsDeleted)
            return Result.Failure<Guid>(ContentErrors.ContentNotFound);

        if (!string.Equals(content.SiteKey, siteKey, StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(content.Locale, locale, StringComparison.OrdinalIgnoreCase))
            return Result.Failure<Guid>(ContentErrors.ContentRouteSiteMismatch);

        if (await routes.ExistsPathAsync(siteKey, locale, path, null, ct))
            return Result.Failure<Guid>(ContentErrors.ContentRoutePathAlreadyExists);

        if (command.IsPrimary)
        {
            var currentPrimary = await routes.GetPrimaryAsync(command.ContentId, locale, ct);
            currentPrimary?.SetPrimary(false, command.ActorUserId);
        }

        var route = ContentRoute.Create(
            siteKey,
            command.ContentId,
            locale,
            path,
            command.IsPrimary,
            command.IsActive,
            command.ActorUserId);

        await routes.AddAsync(route, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success(route.Id);
    }
}
