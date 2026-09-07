using CustomCodeFramework.Core.Pagination;
using CustomCodeFramework.Cqrs.Queries;
using Dhole.Content.Contracts.ContentItems;
using Dhole.Content.Domain.ContentItems.Enums;
namespace Dhole.Content.Application.ContentItems.GetContentItems;
public sealed record GetContentItemsQuery(PageRequest Page,string? SiteKey,ContentType? Type,ContentStatus? Status,string? Search,string? Locale) : IQuery<PagedResult<ContentItemListDto>>;
