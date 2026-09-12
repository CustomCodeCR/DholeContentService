using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Content.Application.Abstractions.Auditing;
using Dhole.Content.Application.Abstractions.Repositories;
using Dhole.Content.Application.Auditing;
using Dhole.Content.Domain.Media;
using Dhole.Content.Domain.Shared;

namespace Dhole.Content.Application.Media.UpdateContentMedia;

public sealed class UpdateContentMediaCommandHandler(
    IContentMediaRepository links,
    IContentAuditService audit,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateContentMediaCommand, Result>
{
    public async Task<Result> HandleAsync(UpdateContentMediaCommand command, CancellationToken cancellationToken = default)
    {
        var link = await links.GetByIdAsync(command.Id, cancellationToken);
        if (link is null || link.IsDeleted) return Result.Failure(ContentErrors.ContentMediaNotFound);

        string normalizedRole;
        try
        {
            normalizedRole = ContentMediaRoles.Normalize(command.Role);
        }
        catch (ArgumentException)
        {
            return Result.Failure(ContentErrors.InvalidContentMedia);
        }

        if (await links.ExistsAsync(link.ContentId, link.MediaReferenceId, normalizedRole, link.Id, cancellationToken))
            return Result.Failure(ContentErrors.ContentMediaAlreadyExists);

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

        try
        {
            link.Update(
                normalizedRole,
                command.SortOrder,
                command.AltTextOverride,
                command.CaptionOverride,
                command.FocalX,
                command.FocalY,
                command.SettingsJson,
                command.ActorUserId);
        }
        catch (ArgumentException)
        {
            return Result.Failure(ContentErrors.InvalidContentMedia);
        }

        await audit.PublishAsync(
            new ContentAuditEvent(
                ContentAuditEventTypes.ContentMediaUpdated,
                ContentAuditActions.Updated,
                ContentAuditEntityTypes.ContentMedia,
                link.Id,
                command.ActorUserId,
                Before: before,
                After: new
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
                }),
            cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
