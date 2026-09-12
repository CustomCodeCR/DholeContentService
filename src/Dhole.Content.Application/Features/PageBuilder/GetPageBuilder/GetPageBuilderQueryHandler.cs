using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Queries;
using Dhole.Content.Application.Abstractions.Repositories;
using Dhole.Content.Contracts.PageBuilder;
using Dhole.Content.Domain.ContentItems.Enums;
using Dhole.Content.Domain.PageBuilder;
using Dhole.Content.Domain.Shared;

namespace Dhole.Content.Application.PageBuilder.GetPageBuilder;

public sealed class GetPageBuilderQueryHandler(IContentItemRepository contents)
    : IQueryHandler<GetPageBuilderQuery, Result<PageBuilderDocumentDto>>
{
    public async Task<Result<PageBuilderDocumentDto>> HandleAsync(GetPageBuilderQuery query, CancellationToken cancellationToken = default)
    {
        var item = await contents.GetByIdWithDetailsAsync(query.ContentId, cancellationToken);
        if (item is null || item.IsDeleted)
            return Result.Failure<PageBuilderDocumentDto>(ContentErrors.ContentNotFound);
        if (item.Type != ContentType.Page)
            return Result.Failure<PageBuilderDocumentDto>(ContentErrors.PageBuilderOnlyPages);

        try
        {
            var normalized = PageBuilderDocument.NormalizeAndValidate(item.BlocksJson);
            return Result.Success(new PageBuilderDocumentDto(item.Id, normalized));
        }
        catch (ArgumentException)
        {
            return Result.Failure<PageBuilderDocumentDto>(ContentErrors.InvalidBlocksJson);
        }
    }
}
