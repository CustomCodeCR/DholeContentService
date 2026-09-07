using Dhole.Content.Application.Abstractions;
using Dhole.Content.Application.Abstractions.Auditing;
using Dhole.Content.Application.Auditing;
using Dhole.Content.Contracts;
using Dhole.Content.Domain.Content;

namespace Dhole.Content.Application.Services;

public sealed class CmsAdministrationService(IContentRepository repository, IContentCache cache, IContentAuditService audit)
{
    public Task<IReadOnlyCollection<TaxonomyTerm>> TaxonomiesAsync(string? siteKey, string? kind, CancellationToken ct)
        => repository.GetTaxonomiesAsync(Site(siteKey), kind, ct);

    public async Task<TaxonomyTerm> CreateTaxonomyAsync(TaxonomyWriteRequest request, CancellationToken ct)
    {
        var term = TaxonomyTerm.Create(request.Kind, request.Name, request.Slug, request.Description, request.ParentId, request.SortOrder, request.SiteKey);
        await repository.AddTaxonomyAsync(term, ct);
        await audit.PublishAsync(new ContentAuditEvent(
            ContentAuditEventTypes.TaxonomyCreated,
            ContentAuditActions.Created,
            ContentAuditEntityTypes.Taxonomy,
            term.Id,
            After: TaxonomyAuditSnapshot.From(term),
            Payload: new { term.Id, term.SiteKey, term.Kind, term.Slug }), ct);
        await repository.SaveChangesAsync(ct);
        return term;
    }

    public async Task<MediaResponse> RegisterMediaAsync(MediaRegisterRequest request, Guid? actor, CancellationToken ct)
    {
        var media = MediaReference.Create(request.StorageFileId, request.FileName, request.ContentType, request.AltText, request.Caption, request.MetadataJson, actor);
        await repository.AddMediaAsync(media, ct);
        await audit.PublishAsync(new ContentAuditEvent(
            ContentAuditEventTypes.MediaRegistered,
            ContentAuditActions.Created,
            ContentAuditEntityTypes.Media,
            media.Id,
            actor,
            After: MediaAuditSnapshot.From(media),
            Payload: new { media.Id, media.StorageFileId, media.FileName, media.ContentType }), ct);
        await repository.SaveChangesAsync(ct);
        return Map(media);
    }

    public async Task<IReadOnlyCollection<MediaResponse>> BrowseMediaAsync(string? search, int page, int pageSize, CancellationToken ct)
        => (await repository.GetMediaAsync(search, Math.Max(1, page), Math.Clamp(pageSize, 1, 100), ct)).Select(Map).ToArray();

    public async Task ReplaceMenuAsync(MenuWriteRequest request, CancellationToken ct)
    {
        var site = Site(request.SiteKey);
        var before = await repository.GetMenuAsync(request.Location, site, ct);
        var menu = NavigationMenu.Create(request.Name, request.Location, site);
        var items = request.Items.Select(x => NavigationMenuItem.Create(menu.Id, x.Label, x.Url, x.ContentId, x.ParentId, x.SortOrder, x.Target)).ToArray();
        await repository.ReplaceMenuAsync(menu, items, ct);
        await audit.PublishAsync(new ContentAuditEvent(
            ContentAuditEventTypes.MenuReplaced,
            ContentAuditActions.Updated,
            ContentAuditEntityTypes.Menu,
            menu.Id,
            Before: before is null ? null : MenuSnapshot(before),
            After: new { menu.Id, menu.SiteKey, menu.Name, menu.Location, Items = items.Select(MenuItemSnapshot).ToArray() },
            Payload: new { menu.Location, ItemCount = items.Length }), ct);
        await repository.SaveChangesAsync(ct);
        await cache.RemoveMenuAsync(site, request.Location, ct);
    }

    public async Task<PublicMenuResponse?> PublicMenuAsync(string location, string? siteKey, CancellationToken ct)
    {
        var menu = await repository.GetMenuAsync(location, Site(siteKey), ct);
        return menu is null ? null : new PublicMenuResponse(menu.Id, menu.Name, menu.Location,
            menu.Items.OrderBy(x => x.SortOrder).Select(x => new PublicMenuItemResponse(x.Id, x.ParentId, x.Label, x.Url, x.ContentId, x.Target, x.SortOrder)).ToArray());
    }

    public async Task UpsertSettingAsync(SiteSettingWriteRequest request, Guid? actor, CancellationToken ct)
    {
        var site = Site(request.SiteKey);
        var setting = await repository.GetSettingAsync(request.Key, site, ct);
        var before = setting is null ? null : SettingAuditSnapshot.From(setting);
        if (setting is null)
        {
            setting = SiteSetting.Create(request.Key, request.ValueJson, request.IsPublic, site, actor);
            await repository.AddSettingAsync(setting, ct);
        }
        else setting.Update(request.ValueJson, request.IsPublic, actor);

        await audit.PublishAsync(new ContentAuditEvent(
            ContentAuditEventTypes.SettingUpserted,
            before is null ? ContentAuditActions.Created : ContentAuditActions.Updated,
            ContentAuditEntityTypes.Setting,
            setting.Id,
            actor,
            Before: before,
            After: SettingAuditSnapshot.From(setting),
            Payload: new { setting.SiteKey, setting.Key, setting.IsPublic }), ct);
        await repository.SaveChangesAsync(ct);
        await cache.RemoveSettingsAsync(site, ct);
    }

    public Task<IReadOnlyCollection<SiteSetting>> SettingsAsync(string? siteKey, bool publicOnly, CancellationToken ct)
        => repository.GetSettingsAsync(Site(siteKey), publicOnly, ct);

    private static object MenuSnapshot(NavigationMenu x) => new
    {
        x.Id, x.SiteKey, x.Name, x.Location, x.IsActive,
        Items = x.Items.OrderBy(i => i.SortOrder).Select(MenuItemSnapshot).ToArray()
    };
    private static object MenuItemSnapshot(NavigationMenuItem x) => new { x.Id, x.ParentId, x.Label, x.Url, x.ContentId, x.Target, x.SortOrder, x.IsVisible };
    private static MediaResponse Map(MediaReference x) => new(x.Id, x.StorageFileId, x.FileName, x.ContentType, x.AltText, x.Caption, x.MetadataJson, x.CreatedAtUtc);
    private static string Site(string? value) => string.IsNullOrWhiteSpace(value) ? "main" : value.Trim();
}
