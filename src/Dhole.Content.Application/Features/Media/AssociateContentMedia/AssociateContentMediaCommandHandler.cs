using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Content.Application.Abstractions.Auditing;
using Dhole.Content.Application.Abstractions.Repositories;
using Dhole.Content.Application.Auditing;
using Dhole.Content.Domain.Media;
using Dhole.Content.Domain.Media.Entities;
using Dhole.Content.Domain.Shared;

namespace Dhole.Content.Application.Media.AssociateContentMedia;

public sealed class AssociateContentMediaCommandHandler(
    IContentItemRepository contents,
    IMediaReferenceRepository media,
    IContentMediaRepository links,
    IContentAuditService audit,
    IUnitOfWork unitOfWork)
    : ICommandHandler<AssociateContentMediaCommand, Result<Guid>>
{
    public async Task<Result<Guid>> HandleAsync(AssociateContentMediaCommand command, CancellationToken cancellationToken = default)
    {
        var content = await contents.GetByIdAsync(command.ContentId, cancellationToken);
        if (content is null || content.IsDeleted) return Result.Failure<Guid>(ContentErrors.ContentNotFound);

        var mediaReference = await media.GetByIdAsync(command.MediaReferenceId, cancellationToken);
        if (mediaReference is null || mediaReference.IsDeleted) return Result.Failure<Guid>(ContentErrors.MediaNotFound);

        string normalizedRole;
        try
        {
            normalizedRole = ContentMediaRoles.Normalize(command.Role);
        }
        catch (ArgumentException)
        {
            return Result.Failure<Guid>(ContentErrors.InvalidContentMedia);
        }

        if (await links.ExistsAsync(command.ContentId, command.MediaReferenceId, normalizedRole, null, cancellationToken))
            return Result.Failure<Guid>(ContentErrors.ContentMediaAlreadyExists);

        ContentMedia link;
        try
        {
            link = ContentMedia.Create(
                command.ContentId,
                command.MediaReferenceId,
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
            return Result.Failure<Guid>(ContentErrors.InvalidContentMedia);
        }

        await links.AddAsync(link, cancellationToken);
        await audit.PublishAsync(
            new ContentAuditEvent(
                ContentAuditEventTypes.ContentMediaCreated,
                ContentAuditActions.Created,
                ContentAuditEntityTypes.ContentMedia,
                link.Id,
                command.ActorUserId,
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
        return Result.Success(link.Id);
    }
}
