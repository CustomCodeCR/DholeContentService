using CustomCodeFramework.Core.Pagination;
using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Content.Contracts.ContentItems;
using Dhole.Content.Domain.ContentItems.Entities;
using Dhole.Content.Domain.ContentItems.Enums;

namespace Dhole.Content.Application.Abstractions.Repositories;

public interface IContentItemRepository : IRepository<ContentItem, Guid>
{
    Task<bool> ExistsBySlugAsync(string siteKey, string locale, string slug, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<ContentItem?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ContentItem?> GetPublishedBySlugAsync(string siteKey, string slug, string? locale, CancellationToken cancellationToken = default);
    Task<PagedResult<ContentItemListDto>> GetPagedAsync(PageRequest page, string? siteKey = null, ContentType? type = null, ContentStatus? status = null, string? search = null, string? locale = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<ContentItem>> GetDueScheduledAsync(DateTime utcNow, int take = 100, CancellationToken cancellationToken = default);
    Task<ContentRevision?> GetRevisionAsync(Guid contentId, Guid revisionId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<ContentRevisionDto>> GetRevisionsAsync(Guid contentId, CancellationToken cancellationToken = default);
}
