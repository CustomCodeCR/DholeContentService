using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Content.Application.Abstractions.Auditing;
using Dhole.Content.Application.Abstractions.Repositories;
using Dhole.Content.Application.Auditing;
using Dhole.Content.Application.Redirects;
using Dhole.Content.Domain.Redirects.Entities;
using Dhole.Content.Domain.Routes.Entities;
using Dhole.Content.Domain.Shared;

namespace Dhole.Content.Application.Routes.UpdateContentRoute;

public sealed class UpdateContentRouteCommandHandler(
    IContentRouteRepository routes,
    IContentItemRepository contents,
    ISiteRepository sites,
    IRedirectRepository redirects,
    IContentAuditService audit,
    IUnitOfWork unitOfWork
) : ICommandHandler<UpdateContentRouteCommand, Result>
{
    public async Task<Result> HandleAsync(UpdateContentRouteCommand command, CancellationToken ct = default)
    {
        var route = await routes.GetByIdAsync(command.Id, ct);
        if (route is null)
            return Result.Failure(ContentErrors.ContentRouteNotFound);

        var siteKey = command.SiteKey.Trim().ToLowerInvariant();
        var locale = command.Locale.Trim();
        var path = ContentRoute.NormalizePath(command.Path);

        var site = await sites.GetBySiteKeyAsync(siteKey, ct);
        if (site is null || site.IsDeleted)
            return Result.Failure(ContentErrors.SiteNotFound);

        var content = await contents.GetByIdWithDetailsAsync(route.ContentId, ct);
        if (content is null || content.IsDeleted)
            return Result.Failure(ContentErrors.ContentNotFound);

        if (!string.Equals(content.SiteKey, siteKey, StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(content.Locale, locale, StringComparison.OrdinalIgnoreCase))
            return Result.Failure(ContentErrors.ContentRouteSiteMismatch);

        if (await routes.ExistsPathAsync(siteKey, locale, path, route.Id, ct))
            return Result.Failure(ContentErrors.ContentRoutePathAlreadyExists);

        if (command.IsPrimary)
        {
            var currentPrimary = await routes.GetPrimaryAsync(route.ContentId, locale, ct);
            if (currentPrimary is not null && currentPrimary.Id != route.Id)
                currentPrimary.SetPrimary(false, command.ActorUserId);
        }

        var oldPath = route.Path;
        var pathChanged = !string.Equals(oldPath, path, StringComparison.OrdinalIgnoreCase);
        route.Update(siteKey, locale, path, command.IsPrimary, command.IsActive, command.ActorUserId);

        if (pathChanged && command.CreatePermanentRedirect)
        {
            var existing = await redirects.GetBySourcePathAsync(siteKey, oldPath, ct);
            if (existing is null)
            {
                var redirect = ContentRedirect.Create(siteKey, oldPath, path, 301, true, null, null, command.ActorUserId);
                await redirects.AddAsync(redirect, ct);
                await audit.PublishAsync(new ContentAuditEvent(ContentAuditEventTypes.RedirectCreated, ContentAuditActions.Created,
                    ContentAuditEntityTypes.Redirect, redirect.Id, command.ActorUserId, After: RedirectAuditSnapshot.From(redirect)), ct);
            }
            else
            {
                var before = RedirectAuditSnapshot.From(existing);
                existing.Update(siteKey, oldPath, path, 301, true, null, null, command.ActorUserId);
                await audit.PublishAsync(new ContentAuditEvent(ContentAuditEventTypes.RedirectUpdated, ContentAuditActions.Updated,
                    ContentAuditEntityTypes.Redirect, existing.Id, command.ActorUserId, Before: before,
                    After: RedirectAuditSnapshot.From(existing)), ct);
            }
        }

        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
