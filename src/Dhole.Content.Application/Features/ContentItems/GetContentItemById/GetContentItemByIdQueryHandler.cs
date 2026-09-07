using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Queries;
using Dhole.Content.Application.Abstractions.Repositories;
using Dhole.Content.Application.Mappings;
using Dhole.Content.Contracts.ContentItems;
using Dhole.Content.Domain.Shared;
namespace Dhole.Content.Application.ContentItems.GetContentItemById;
public sealed class GetContentItemByIdQueryHandler(IContentItemRepository contents):IQueryHandler<GetContentItemByIdQuery,Result<ContentItemDto>>
{ public async Task<Result<ContentItemDto>> HandleAsync(GetContentItemByIdQuery query,CancellationToken cancellationToken=default){var item=await contents.GetByIdWithDetailsAsync(query.Id,cancellationToken);return item is null||item.IsDeleted?Result.Failure<ContentItemDto>(ContentErrors.ContentNotFound):Result.Success(ContentMappings.ToDto(item));} }
