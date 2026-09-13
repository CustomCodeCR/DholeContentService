using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Content.Application.Abstractions.Auditing;
using Dhole.Content.Application.Abstractions.Cache;
using Dhole.Content.Application.Abstractions.Repositories;
using Dhole.Content.Application.Auditing;
using Dhole.Content.Domain.Navigation.Entities;
using Dhole.Content.Domain.Shared;

namespace Dhole.Content.Application.Navigation.UpsertNavigationMenu;

public sealed class UpsertNavigationMenuCommandHandler(
    INavigationMenuRepository repo,
    IContentAuditService audit,
    IContentCacheService cache,
    IUnitOfWork uow) : ICommandHandler<UpsertNavigationMenuCommand, Result<Guid>>
{
    public async Task<Result<Guid>> HandleAsync(UpsertNavigationMenuCommand c, CancellationToken ct = default)
    {
        var site = string.IsNullOrWhiteSpace(c.SiteKey) ? ContentConstants.DefaultSiteKey : c.SiteKey.Trim();
        var location = c.Location.Trim().ToLowerInvariant();
        var menu = await repo.GetByLocationAsync(site, location, ct);
        object? before = null;
        string[] previousOrder = [];

        if (menu is null)
        {
            menu = NavigationMenu.Create(c.Name, location, site, c.ActorUserId);
            await repo.AddAsync(menu, ct);
        }
        else
        {
            before = ContentAuditSnapshots.From(menu);
            previousOrder = menu.Items
                .OrderBy(item => item.SortOrder)
                .Select(item => BuildOrderKey(item.Label, item.Url, item.ContentId, item.ParentId, item.Target))
                .ToArray();
        }

        var requestedOrder = c.Items
            .OrderBy(item => item.SortOrder)
            .Select(item => BuildOrderKey(item.Label, item.Url, item.ContentId, item.ParentId, item.Target))
            .ToArray();
        var reordered = before is not null
            && previousOrder.Length == requestedOrder.Length
            && previousOrder.Order(StringComparer.Ordinal).SequenceEqual(requestedOrder.Order(StringComparer.Ordinal), StringComparer.Ordinal)
            && !previousOrder.SequenceEqual(requestedOrder, StringComparer.Ordinal);

        menu.Replace(c.Name, c.Items.Select(i => (i.Label, i.Url, i.ContentId, i.ParentId, i.SortOrder, i.Target)), c.ActorUserId);
        var action = before is null
            ? ContentAuditActions.Created
            : ContentAuditActions.ResolveMutation(reordered: reordered);
        await audit.PublishAsync(new ContentAuditEvent(
            ContentAuditEventTypes.MenuUpdated,
            action,
            ContentAuditEntityTypes.NavigationMenu,
            menu.Id,
            c.ActorUserId,
            Before: before,
            After: ContentAuditSnapshots.From(menu)), ct);
        await uow.SaveChangesAsync(ct);
        await cache.RemoveMenuAsync(site, location, ct);
        return Result.Success(menu.Id);
    }

    private static string BuildOrderKey(string label, string? url, Guid? contentId, Guid? parentId, string? target)
        => string.Join('|', label.Trim(), url?.Trim() ?? string.Empty, contentId?.ToString() ?? string.Empty,
            parentId?.ToString() ?? string.Empty, target?.Trim() ?? "_self");
}
