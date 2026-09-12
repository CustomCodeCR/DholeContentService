using CustomCodeFramework.Core.Pagination;
using CustomCodeFramework.Postgres.EntityFramework.Repositories;
using Dhole.Content.Application.Abstractions.Repositories;
using Dhole.Content.Contracts.Media;
using Dhole.Content.Domain.Media.Entities;
using Dhole.Content.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Dhole.Content.Persistence.Repositories;

public sealed class MediaReferenceRepository(ServiceDbContext db)
    : EfRepository<MediaReference, Guid>(db), IMediaReferenceRepository
{
    public Task<bool> ExistsByStorageFileIdAsync(
        Guid storageFileId,
        CancellationToken cancellationToken = default)
        => db.MediaReferences.AnyAsync(
            x => x.StorageFileId == storageFileId && !x.IsDeleted,
            cancellationToken);

    public async Task<PagedResult<MediaDto>> GetPagedAsync(
        PageRequest page,
        string? search = null,
        string? contentType = null,
        CancellationToken cancellationToken = default)
    {
        var query = db.MediaReferences.AsNoTracking().Where(x => !x.IsDeleted);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var value = search.Trim().ToLower();
            query = query.Where(x =>
                x.FileName.ToLower().Contains(value)
                || (x.AltText != null && x.AltText.ToLower().Contains(value))
                || (x.Caption != null && x.Caption.ToLower().Contains(value)));
        }

        if (!string.IsNullOrWhiteSpace(contentType))
        {
            var type = contentType.Trim().ToLower();
            if (type == "document")
            {
                query = query.Where(x =>
                    !x.ContentType.ToLower().StartsWith("image/")
                    && !x.ContentType.ToLower().StartsWith("video/")
                    && x.ContentType.ToLower() != "application/pdf");
            }
            else
            {
                query = query.Where(x => x.ContentType.ToLower().StartsWith(type));
            }
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .Skip(page.Skip)
            .Take(page.PageSize)
            .Select(x => new MediaDto(
                x.Id,
                x.StorageFileId,
                x.FileName,
                x.ContentType,
                x.AltText,
                x.Caption,
                x.MetadataJson,
                x.CreatedAtUtc,
                x.UpdatedAtUtc))
            .ToListAsync(cancellationToken);

        return PagedResult<MediaDto>.Create(items, page.PageNumber, page.PageSize, total);
    }
}
