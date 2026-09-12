using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Content.Application.Abstractions.Auditing;
using Dhole.Content.Application.Abstractions.Repositories;
using Dhole.Content.Application.Auditing;
using Dhole.Content.Domain.Shared;

namespace Dhole.Content.Application.Media.RemoveContentMedia;

public sealed class RemoveContentMediaCommandHandler(
    IContentMediaRepository links,
    IContentAuditService audit,
    IUnitOfWork unitOfWork)
    : ICommandHandler<RemoveContentMediaCommand, Result>
{
    public async Task<Result> HandleAsync(RemoveContentMediaCommand command, CancellationToken cancellationToken = default)
    {
        var link = await links.GetByIdAsync(command.Id, cancellationToken);
        if (link is null || link.IsDeleted || link.ContentId != command.ContentId)
            return Result.Failure(ContentErrors.ContentMediaNotFound);

        var before = new
        {
            link.Id,
            link.ContentId,
            link.MediaReferenceId,
            link.Role,
            link.SortOrder,
            link.AltTextOverride,
            link.CaptionOverride,
            link.FocalX,
            link.FocalY,
            link.SettingsJson
        };

        link.Delete(command.ActorUserId);
        await audit.PublishAsync(
            new ContentAuditEvent(
                ContentAuditEventTypes.ContentMediaDeleted,
                ContentAuditActions.Deleted,
                ContentAuditEntityTypes.ContentMedia,
                link.Id,
                command.ActorUserId,
                Before: before,
                Payload: new { link.ContentId, link.MediaReferenceId, link.Role }),
            cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
