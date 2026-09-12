using CustomCodeFramework.Core.Pagination;
using CustomCodeFramework.Postgres.EntityFramework.Repositories;
using Dhole.Content.Application.Abstractions.Repositories;
using Dhole.Content.Contracts.ContentItems;
using Dhole.Content.Domain.ContentItems.Entities;
using Dhole.Content.Domain.ContentItems.Enums;
using Dhole.Content.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Dhole.Content.Persistence.Repositories;

public sealed class ContentItemRepository(ServiceDbContext db) : EfRepository<ContentItem, Guid>(db), IContentItemRepository
{
    public Task<bool> ExistsBySlugAsync(string siteKey, string locale, string slug, Guid? excludeId = null, CancellationToken ct = default)
    {
        var s = siteKey.Trim();
        var l = locale.Trim();
        var v = slug.Trim().ToLowerInvariant();
        return db.ContentItems.AnyAsync(
            x => x.SiteKey == s && x.Locale == l && x.Slug == v && !x.IsDeleted && (!excludeId.HasValue || x.Id != excludeId.Value),
            ct);
    }

    public Task<ContentItem?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default) =>
        db.ContentItems.Include(x => x.Revisions).Include(x => x.Taxonomies).FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<ContentItem?> GetPublishedBySlugAsync(string siteKey, string slug, string? locale, CancellationToken ct = default)
    {
        var s = siteKey.Trim();
        var v = slug.Trim().ToLowerInvariant();
        var q = db.ContentItems.AsNoTracking().Where(x => x.SiteKey == s && x.Slug == v && !x.IsDeleted && x.Status == ContentStatus.Published);
        if (!string.IsNullOrWhiteSpace(locale))
        {
            var l = locale.Trim();
            q = q.Where(x => x.Locale == l);
        }
        return q.FirstOrDefaultAsync(ct);
    }

    public async Task<PagedResult<ContentItemListDto>> GetPagedAsync(PageRequest page, string? siteKey = null, ContentType? type = null, ContentStatus? status = null, string? search = null, string? locale = null, CancellationToken ct = default)
    {
        var q = db.ContentItems.AsNoTracking().Where(x => !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(siteKey))
        {
            var s = siteKey.Trim();
            q = q.Where(x => x.SiteKey == s);
        }
        if (type.HasValue) q = q.Where(x => x.Type == type.Value);
        if (status.HasValue) q = q.Where(x => x.Status == status.Value);
        if (!string.IsNullOrWhiteSpace(locale))
        {
            var l = locale.Trim();
            q = q.Where(x => x.Locale == l);
        }
        if (!string.IsNullOrWhiteSpace(search))
        {
            var v = search.Trim().ToLower();
            q = q.Where(x => x.Title.ToLower().Contains(v) || x.Slug.ToLower().Contains(v) || (x.Excerpt != null && x.Excerpt.ToLower().Contains(v)));
        }
        var total = await q.CountAsync(ct);
        var items = await q
            .OrderByDescending(x => x.PublishedAtUtc ?? x.CreatedAtUtc)
            .ThenBy(x => x.SortOrder)
            .Skip(page.Skip)
            .Take(page.PageSize)
            .Select(x => new ContentItemListDto(x.Id, x.SiteKey, x.Type.ToString(), x.Status.ToString(), x.Title, x.Slug, x.Excerpt, x.FeaturedMediaId, x.Locale, x.IsFeatured, x.PublishedAtUtc, x.CreatedAtUtc, x.UpdatedAtUtc))
            .ToListAsync(ct);
        return PagedResult<ContentItemListDto>.Create(items, page.PageNumber, page.PageSize, total);
    }

    public async Task<IReadOnlyCollection<ContentItem>> GetDueScheduledAsync(DateTime utcNow, int take = 100, CancellationToken ct = default) =>
        await db.ContentItems
            .Where(x => !x.IsDeleted && x.Status == ContentStatus.Scheduled && x.ScheduledAtUtc != null && x.ScheduledAtUtc <= utcNow)
            .OrderBy(x => x.ScheduledAtUtc)
            .Take(take)
            .ToListAsync(ct);

    public async Task<IReadOnlyCollection<ContentItem>> GetDueUnpublishAsync(DateTime utcNow, int take = 100, CancellationToken ct = default) =>
        await db.ContentItems
            .Where(x => !x.IsDeleted && x.Status == ContentStatus.Published && x.UnpublishAtUtc != null && x.UnpublishAtUtc <= utcNow)
            .OrderBy(x => x.UnpublishAtUtc)
            .Take(take)
            .ToListAsync(ct);

    public Task<ContentRevision?> GetRevisionAsync(Guid contentId, Guid revisionId, CancellationToken ct = default) =>
        db.ContentRevisions.FirstOrDefaultAsync(x => x.ContentId == contentId && x.Id == revisionId, ct);

    public async Task<IReadOnlyCollection<ContentRevisionDto>> GetRevisionsAsync(Guid contentId, CancellationToken ct = default) =>
        await db.ContentRevisions.AsNoTracking()
            .Where(x => x.ContentId == contentId)
            .OrderByDescending(x => x.RevisionNumber)
            .Select(x => new ContentRevisionDto(x.Id, x.RevisionNumber, x.Title, x.Slug, x.Reason, x.CreatedBy, x.CreatedAtUtc))
            .ToListAsync(ct);
}
