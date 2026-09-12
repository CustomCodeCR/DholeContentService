using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Content.Application.Abstractions.Auditing;
using Dhole.Content.Application.Abstractions.Cache;
using Dhole.Content.Application.Abstractions.Repositories;
using Dhole.Content.Application.Auditing;
using Dhole.Content.Domain.Seo;
using Dhole.Content.Domain.Shared;

namespace Dhole.Content.Application.Seo;

public sealed record UpdateSeoCommand(
    Guid ContentId,
    string? Title,
    string? Description,
    string? Keywords,
    string? CanonicalUrl,
    string? Robots,
    Guid? OpenGraphMediaId,
    string? StructuredDataJson,
    Guid? ActorUserId) : ICommand<Result>;

public sealed class UpdateSeoCommandHandler(
    IContentItemRepository contents,
    IMediaReferenceRepository media,
    IContentAuditService audit,
    IContentCacheService cache,
    IUnitOfWork unitOfWork) : ICommandHandler<UpdateSeoCommand, Result>
{
    public async Task<Result> HandleAsync(UpdateSeoCommand command, CancellationToken cancellationToken = default)
    {
        var item = await contents.GetByIdWithDetailsAsync(command.ContentId, cancellationToken);
        if (item is null || item.IsDeleted) return Result.Failure(ContentErrors.ContentNotFound);

        if (command.OpenGraphMediaId.HasValue)
        {
            var mediaReference = await media.GetByIdAsync(command.OpenGraphMediaId.Value, cancellationToken);
            if (mediaReference is null || mediaReference.IsDeleted) return Result.Failure(ContentErrors.MediaNotFound);
        }

        NormalizedSeo seo;
        try
        {
            seo = SeoRules.Normalize(command.Title, command.Description, command.Keywords, command.CanonicalUrl,
                command.Robots, command.OpenGraphMediaId, command.StructuredDataJson);
        }
        catch (ArgumentException)
        {
            return Result.Failure(ContentErrors.InvalidSeoData);
        }

        var before = ContentAuditSnapshots.From(item);
        item.SetSeo(seo.Title, seo.Description, seo.Keywords, seo.CanonicalUrl, seo.Robots,
            seo.OpenGraphMediaId, seo.StructuredDataJson, command.ActorUserId);

        await audit.PublishAsync(new ContentAuditEvent(
            ContentAuditEventTypes.SeoUpdated,
            ContentAuditActions.Updated,
            ContentAuditEntityTypes.ContentItem,
            item.Id,
            command.ActorUserId,
            Before: before,
            After: ContentAuditSnapshots.From(item)), cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await cache.RemoveContentAsync(item.SiteKey, item.Slug, cancellationToken);
        return Result.Success();
    }
}
