using CustomCodeFramework.Postgres.EntityFramework.Repositories;
using Dhole.Content.Application.Abstractions.Repositories;
using Dhole.Content.Contracts.Media;
using Dhole.Content.Domain.Media;
using Dhole.Content.Domain.Media.Entities;
using Dhole.Content.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Dhole.Content.Persistence.Repositories;

public sealed class ContentMediaRepository(ServiceDbContext db)
    : EfRepository<ContentMedia, Guid>(db), IContentMediaRepository
{
    public Task<bool> ExistsAsync(
        Guid contentId,
        Guid mediaReferenceId,
        string role,
        Guid? excludeId = null,
        CancellationToken ct = default)
    {
        var normalizedRole = ContentMediaRoles.Normalize(role);
        return db.ContentMedia.AnyAsync(
            x => x.ContentId == contentId &&
                 x.MediaReferenceId == mediaReferenceId &&
                 x.Role == normalizedRole &&
                 !x.IsDeleted &&
                 (!excludeId.HasValue || x.Id != excludeId.Value),
            ct);
    }

    public async Task<IReadOnlyCollection<ContentMediaDto>> GetByContentAsync(
        Guid contentId,
        string? role = null,
        CancellationToken ct = default)
    {
        var query =
            from link in db.ContentMedia.AsNoTracking()
            join media in db.MediaReferences.AsNoTracking()
                on link.MediaReferenceId equals media.Id
            where link.ContentId == contentId && !link.IsDeleted && !media.IsDeleted
            select new { link, media };

        if (!string.IsNullOrWhiteSpace(role))
        {
            var normalizedRole = ContentMediaRoles.Normalize(role);
            query = query.Where(x => x.link.Role == normalizedRole);
        }

        return await query
            .OrderBy(x => x.link.Role)
            .ThenBy(x => x.link.SortOrder)
            .ThenBy(x => x.link.CreatedAtUtc)
            .Select(x => new ContentMediaDto(
                x.link.Id,
                x.link.ContentId,
                x.link.MediaReferenceId,
                x.link.Role,
                x.link.SortOrder,
                x.link.AltTextOverride,
                x.link.CaptionOverride,
                x.link.FocalX,
                x.link.FocalY,
                x.link.SettingsJson,
                x.media.StorageFileId,
                x.media.FileName,
                x.media.ContentType,
                x.media.AltText,
                x.media.Caption))
            .ToListAsync(ct);
    }
}
