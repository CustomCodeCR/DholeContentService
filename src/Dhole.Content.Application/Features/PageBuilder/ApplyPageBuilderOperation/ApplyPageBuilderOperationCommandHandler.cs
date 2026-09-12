using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Content.Application.Abstractions.Auditing;
using Dhole.Content.Application.Abstractions.Cache;
using Dhole.Content.Application.Abstractions.Repositories;
using Dhole.Content.Application.Auditing;
using Dhole.Content.Contracts.PageBuilder;
using Dhole.Content.Domain.ContentItems.Enums;
using Dhole.Content.Domain.PageBuilder;
using Dhole.Content.Domain.Shared;

namespace Dhole.Content.Application.PageBuilder.ApplyPageBuilderOperation;

public sealed class ApplyPageBuilderOperationCommandHandler(
    IContentItemRepository contents,
    IContentAuditService audit,
    IContentCacheService cache,
    IUnitOfWork unitOfWork
) : ICommandHandler<ApplyPageBuilderOperationCommand, Result<PageBuilderDocumentDto>>
{
    public async Task<Result<PageBuilderDocumentDto>> HandleAsync(
        ApplyPageBuilderOperationCommand command,
        CancellationToken cancellationToken = default)
    {
        var item = await contents.GetByIdWithDetailsAsync(command.ContentId, cancellationToken);
        if (item is null || item.IsDeleted)
            return Result.Failure<PageBuilderDocumentDto>(ContentErrors.ContentNotFound);
        if (item.Type != ContentType.Page)
            return Result.Failure<PageBuilderDocumentDto>(ContentErrors.PageBuilderOnlyPages);
        if (item.Status == ContentStatus.PendingReview)
            return Result.Failure<PageBuilderDocumentDto>(ContentErrors.InvalidContentState);

        string normalized;
        try
        {
            normalized = PageBuilderDocument.ApplyOperation(
                item.BlocksJson,
                command.Operation,
                command.BlockId,
                command.BlockType,
                command.TargetIndex,
                command.IsVisible,
                command.DataJson);
        }
        catch (KeyNotFoundException)
        {
            return Result.Failure<PageBuilderDocumentDto>(ContentErrors.PageBuilderBlockNotFound);
        }
        catch (ArgumentException)
        {
            return Result.Failure<PageBuilderDocumentDto>(ContentErrors.InvalidPageBuilderOperation);
        }

        var before = ContentAuditSnapshots.From(item);
        item.CreateRevision(command.ActorUserId, $"page-builder:{command.Operation}");
        item.Update(
            item.Title,
            item.Slug,
            item.Excerpt,
            normalized,
            item.RenderedHtml,
            item.FeaturedMediaId,
            item.SortOrder,
            item.IsFeatured,
            item.Locale,
            command.ActorUserId);

        await audit.PublishAsync(
            new ContentAuditEvent(
                ContentAuditEventTypes.ContentUpdated,
                ContentAuditActions.Updated,
                ContentAuditEntityTypes.ContentItem,
                item.Id,
                command.ActorUserId,
                Before: before,
                After: ContentAuditSnapshots.From(item),
                Payload: new
                {
                    Operation = command.Operation,
                    command.BlockId,
                    command.BlockType,
                    command.TargetIndex,
                    command.IsVisible
                }),
            cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await cache.RemoveContentAsync(item.SiteKey, item.Slug, cancellationToken);

        return Result.Success(new PageBuilderDocumentDto(item.Id, normalized));
    }
}
