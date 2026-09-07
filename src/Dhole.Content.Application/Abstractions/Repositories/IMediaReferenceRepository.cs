using CustomCodeFramework.Core.Pagination;
using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Content.Contracts.Media;
using Dhole.Content.Domain.Media.Entities;
namespace Dhole.Content.Application.Abstractions.Repositories;
public interface IMediaReferenceRepository : IRepository<MediaReference,Guid>
{
    Task<bool> ExistsByStorageFileIdAsync(Guid storageFileId,CancellationToken cancellationToken=default);
    Task<PagedResult<MediaDto>> GetPagedAsync(PageRequest page,string? search=null,string? contentType=null,CancellationToken cancellationToken=default);
}
