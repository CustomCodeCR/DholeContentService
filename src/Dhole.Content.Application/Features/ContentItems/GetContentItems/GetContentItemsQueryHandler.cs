using CustomCodeFramework.Core.Pagination;
using CustomCodeFramework.Cqrs.Queries;
using Dhole.Content.Application.Abstractions.Repositories;
using Dhole.Content.Contracts.ContentItems;
namespace Dhole.Content.Application.ContentItems.GetContentItems;
public sealed class GetContentItemsQueryHandler(IContentItemRepository contents) : IQueryHandler<GetContentItemsQuery,PagedResult<ContentItemListDto>>
{ public Task<PagedResult<ContentItemListDto>> HandleAsync(GetContentItemsQuery query,CancellationToken cancellationToken=default)=>contents.GetPagedAsync(query.Page,query.SiteKey,query.Type,query.Status,query.Search,query.Locale,cancellationToken); }
