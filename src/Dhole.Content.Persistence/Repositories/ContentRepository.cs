using Dhole.Content.Application.Abstractions;
using Dhole.Content.Domain.Content;
using Dhole.Content.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Dhole.Content.Persistence.Repositories;

public sealed class ContentRepository(ServiceDbContext db) : IContentRepository
{
    public Task<ContentItem?> GetAsync(Guid id, bool includeRevisions, CancellationToken ct)
    {
        IQueryable<ContentItem> query = db.ContentItems.Include(x => x.Taxonomies).ThenInclude(x => x.TaxonomyTerm);
        if (includeRevisions) query = query.Include(x => x.Revisions);
        return query.FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public Task<ContentItem?> GetPublishedBySlugAsync(string slug, string siteKey, string? locale, CancellationToken ct)
        => db.ContentItems.AsNoTracking()
            .Include(x => x.Taxonomies).ThenInclude(x => x.TaxonomyTerm)
            .Where(x => x.SiteKey == siteKey && x.Slug == slug && x.Status == ContentStatus.Published)
            .Where(x => locale == null || x.Locale == locale)
            .FirstOrDefaultAsync(ct);

    public async Task<(IReadOnlyCollection<ContentItem> Items, long Total)> BrowseAsync(
        string? search, ContentType? type, ContentStatus? status, string siteKey, int page, int pageSize, CancellationToken ct)
    {
        var query = db.ContentItems.AsNoTracking().Where(x => x.SiteKey == siteKey);
        if (type.HasValue) query = query.Where(x => x.Type == type.Value);
        if (status.HasValue) query = query.Where(x => x.Status == status.Value);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(x => EF.Functions.ILike(x.Title, pattern) || EF.Functions.ILike(x.Slug, pattern) || (x.Excerpt != null && EF.Functions.ILike(x.Excerpt, pattern)));
        }

        var total = await query.LongCountAsync(ct);
        var items = await query.OrderByDescending(x => x.UpdatedAtUtc)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, total);
    }

    public async Task<IReadOnlyCollection<ContentItem>> GetScheduledDueAsync(DateTime utcNow, CancellationToken ct)
        => await db.ContentItems.Where(x => x.Status == ContentStatus.Scheduled && x.ScheduledAtUtc <= utcNow).ToListAsync(ct);

    public Task<bool> SlugExistsAsync(string slug, string siteKey, Guid? exceptId, CancellationToken ct)
        => db.ContentItems.AnyAsync(x => x.SiteKey == siteKey && x.Slug == slug && (!exceptId.HasValue || x.Id != exceptId.Value), ct);

    public Task AddAsync(ContentItem item, CancellationToken ct) => db.ContentItems.AddAsync(item, ct).AsTask();
    public void AddRevision(ContentRevision revision) => db.ContentRevisions.Add(revision);

    public Task<ContentRevision?> GetRevisionAsync(Guid contentId, Guid revisionId, CancellationToken ct)
        => db.ContentRevisions.AsNoTracking().FirstOrDefaultAsync(x => x.ContentId == contentId && x.Id == revisionId, ct);

    public async Task<IReadOnlyCollection<ContentRevision>> GetRevisionsAsync(Guid contentId, CancellationToken ct)
        => await db.ContentRevisions.AsNoTracking().Where(x => x.ContentId == contentId).OrderByDescending(x => x.RevisionNumber).ToListAsync(ct);

    public async Task ReplaceTaxonomiesAsync(Guid contentId, IReadOnlyCollection<Guid> ids, CancellationToken ct)
    {
        var existing = await db.ContentTaxonomies.Where(x => x.ContentId == contentId).ToListAsync(ct);
        db.ContentTaxonomies.RemoveRange(existing);
        if (ids.Count > 0)
            await db.ContentTaxonomies.AddRangeAsync(ids.Select(x => new ContentTaxonomy { ContentId = contentId, TaxonomyTermId = x }), ct);
    }

    public async Task<IReadOnlyCollection<TaxonomyTerm>> GetTaxonomiesAsync(string siteKey, string? kind, CancellationToken ct)
    {
        var query = db.TaxonomyTerms.AsNoTracking().Where(x => x.SiteKey == siteKey);
        if (!string.IsNullOrWhiteSpace(kind)) query = query.Where(x => x.Kind == kind.ToLower());
        return await query.OrderBy(x => x.Kind).ThenBy(x => x.SortOrder).ThenBy(x => x.Name).ToListAsync(ct);
    }

    public Task AddTaxonomyAsync(TaxonomyTerm term, CancellationToken ct) => db.TaxonomyTerms.AddAsync(term, ct).AsTask();

    public Task AddMediaAsync(MediaReference media, CancellationToken ct) => db.MediaReferences.AddAsync(media, ct).AsTask();

    public async Task<IReadOnlyCollection<MediaReference>> GetMediaAsync(string? search, int page, int pageSize, CancellationToken ct)
    {
        var query = db.MediaReferences.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(x => EF.Functions.ILike(x.FileName, pattern) || (x.AltText != null && EF.Functions.ILike(x.AltText, pattern)));
        }
        return await query.OrderByDescending(x => x.CreatedAtUtc).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
    }

    public Task<NavigationMenu?> GetMenuAsync(string location, string siteKey, CancellationToken ct)
        => db.NavigationMenus.AsNoTracking().Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Location == location.ToLower() && x.SiteKey == siteKey && x.IsActive, ct);

    public async Task ReplaceMenuAsync(NavigationMenu menu, IReadOnlyCollection<NavigationMenuItem> items, CancellationToken ct)
    {
        var existing = await db.NavigationMenus.Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.SiteKey == menu.SiteKey && x.Location == menu.Location, ct);
        if (existing is not null) db.NavigationMenus.Remove(existing);
        await db.NavigationMenus.AddAsync(menu, ct);
        await db.NavigationMenuItems.AddRangeAsync(items, ct);
    }

    public async Task<IReadOnlyCollection<SiteSetting>> GetSettingsAsync(string siteKey, bool publicOnly, CancellationToken ct)
    {
        var query = db.SiteSettings.AsNoTracking().Where(x => x.SiteKey == siteKey);
        if (publicOnly) query = query.Where(x => x.IsPublic);
        return await query.OrderBy(x => x.Key).ToListAsync(ct);
    }

    public Task<SiteSetting?> GetSettingAsync(string key, string siteKey, CancellationToken ct)
        => db.SiteSettings.FirstOrDefaultAsync(x => x.SiteKey == siteKey && x.Key == key, ct);

    public Task AddSettingAsync(SiteSetting setting, CancellationToken ct) => db.SiteSettings.AddAsync(setting, ct).AsTask();
    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
