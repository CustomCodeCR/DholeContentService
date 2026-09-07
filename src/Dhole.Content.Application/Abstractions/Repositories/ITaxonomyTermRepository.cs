using CustomCodeFramework.Core.Pagination;
using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Content.Contracts.Taxonomies;
using Dhole.Content.Domain.Taxonomies.Entities;
namespace Dhole.Content.Application.Abstractions.Repositories;
public interface ITaxonomyTermRepository : IRepository<TaxonomyTerm,Guid>
{
    Task<bool> ExistsBySlugAsync(string siteKey,string kind,string slug,Guid? excludeId=null,CancellationToken cancellationToken=default);
    Task<PagedResult<TaxonomyTermDto>> GetPagedAsync(PageRequest page,string? siteKey=null,string? kind=null,string? search=null,CancellationToken cancellationToken=default);
}
