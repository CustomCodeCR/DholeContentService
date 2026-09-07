using Dhole.Content.Application.Abstractions;
using Dhole.Content.Contracts;
using Dhole.Content.Domain.Content;

namespace Dhole.Content.Application.Services;

public sealed class CmsAdministrationService(IContentRepository repository, IContentCache cache)
{
    public async Task<IReadOnlyCollection<TaxonomyTerm>> TaxonomiesAsync(string? siteKey, string? kind, CancellationToken ct)
        => await repository.GetTaxonomiesAsync(Site(siteKey), kind, ct);

    public async Task<TaxonomyTerm> CreateTaxonomyAsync(TaxonomyWriteRequest request, CancellationToken ct)
    {
        var term = TaxonomyTerm.Create(request.Kind, request.Name, request.Slug, request.Description, request.ParentId, request.SortOrder, request.SiteKey);
        await repository.AddTaxonomyAsync(term, ct);
        await repository.SaveChangesAsync(ct);
        return term;
    }

    public async Task<MediaResponse> RegisterMediaAsync(MediaRegisterRequest request, Guid? actor, CancellationToken ct)
    {
        var media = MediaReference.Create(request.StorageFileId, request.FileName, request.ContentType, request.AltText, request.Caption, request.MetadataJson, actor);
        await repository.AddMediaAsync(media, ct);
        await repository.SaveChangesAsync(ct);
        return Map(media);
    }

    public async Task<IReadOnlyCollection<MediaResponse>> BrowseMediaAsync(string? search, int page, int pageSize, CancellationToken ct)
        => (await repository.GetMediaAsync(search, Math.Max(1, page), Math.Clamp(pageSize, 1, 100), ct)).Select(Map).ToArray();

    public async Task ReplaceMenuAsync(MenuWriteRequest request, CancellationToken ct)
    {
        var menu = NavigationMenu.Create(request.Name, request.Location, request.SiteKey);
        var items = request.Items.Select(x => NavigationMenuItem.Create(menu.Id, x.Label, x.Url, x.ContentId, x.ParentId, x.SortOrder, x.Target)).ToArray();
        await repository.ReplaceMenuAsync(menu, items, ct);
        await repository.SaveChangesAsync(ct);
        await cache.RemoveMenuAsync(Site(request.SiteKey), request.Location, ct);
    }

    public async Task<PublicMenuResponse?> PublicMenuAsync(string location, string? siteKey, CancellationToken ct)
    {
        var menu = await repository.GetMenuAsync(location, Site(siteKey), ct);
        return menu is null ? null : new PublicMenuResponse(
            menu.Id, menu.Name, menu.Location,
            menu.Items.OrderBy(x => x.SortOrder)
                .Select(x => new PublicMenuItemResponse(x.Id, x.ParentId, x.Label, x.Url, x.ContentId, x.Target, x.SortOrder)).ToArray());
    }

    public async Task UpsertSettingAsync(SiteSettingWriteRequest request, Guid? actor, CancellationToken ct)
    {
        var site = Site(request.SiteKey);
        var setting = await repository.GetSettingAsync(request.Key, site, ct);
        if (setting is null)
            await repository.AddSettingAsync(SiteSetting.Create(request.Key, request.ValueJson, request.IsPublic, site, actor), ct);
        else
            setting.Update(request.ValueJson, request.IsPublic, actor);

        await repository.SaveChangesAsync(ct);
        await cache.RemoveSettingsAsync(site, ct);
    }

    public Task<IReadOnlyCollection<SiteSetting>> SettingsAsync(string? siteKey, bool publicOnly, CancellationToken ct)
        => repository.GetSettingsAsync(Site(siteKey), publicOnly, ct);

    private static MediaResponse Map(MediaReference x)
        => new(x.Id, x.StorageFileId, x.FileName, x.ContentType, x.AltText, x.Caption, x.MetadataJson, x.CreatedAtUtc);

    private static string Site(string? value) => string.IsNullOrWhiteSpace(value) ? "main" : value.Trim();
}
