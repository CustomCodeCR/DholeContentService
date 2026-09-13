using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Content.Application.Abstractions.Auditing;
using Dhole.Content.Application.Abstractions.Repositories;
using Dhole.Content.Application.Auditing;
using Dhole.Content.Domain.Shared;
using Dhole.Content.Domain.Sites.Entities;

namespace Dhole.Content.Application.Sites.UpsertSite;

public sealed class UpsertSiteCommandHandler(
    ISiteRepository sites,
    IContentAuditService audit,
    IUnitOfWork unitOfWork
) : ICommandHandler<UpsertSiteCommand, Result<Guid>>
{
    public async Task<Result<Guid>> HandleAsync(
        UpsertSiteCommand command,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(command.SiteKey)
            || string.IsNullOrWhiteSpace(command.Name)
            || string.IsNullOrWhiteSpace(command.PrimaryDomain)
            || string.IsNullOrWhiteSpace(command.DefaultLocale)
            || string.IsNullOrWhiteSpace(command.TimeZone))
        {
            return Result.Failure<Guid>(ContentErrors.InvalidSiteData);
        }

        var key = command.SiteKey.Trim().ToLowerInvariant();
        var existing = await sites.GetBySiteKeyAsync(key, cancellationToken);

        if (existing is null && await sites.ExistsBySiteKeyAsync(key, null, cancellationToken))
        {
            return Result.Failure<Guid>(ContentErrors.SiteKeyAlreadyExists);
        }

        if (await sites.ExistsByPrimaryDomainAsync(command.PrimaryDomain, existing?.Id, cancellationToken))
        {
            return Result.Failure<Guid>(ContentErrors.SiteDomainAlreadyExists);
        }

        var wasCreated = existing is null;
        object? before = existing is null ? null : Snapshot(existing);
        var previousStatus = existing?.Status;

        try
        {
            if (existing is null)
            {
                existing = Site.Create(
                    key,
                    command.Name,
                    command.PrimaryDomain,
                    command.DefaultLocale,
                    command.TimeZone,
                    command.LogoMediaId,
                    command.FaviconMediaId,
                    command.DefaultOpenGraphMediaId,
                    command.Status,
                    command.ActorUserId
                );
                await sites.AddAsync(existing, cancellationToken);
            }
            else
            {
                existing.Update(
                    command.Name,
                    command.PrimaryDomain,
                    command.DefaultLocale,
                    command.TimeZone,
                    command.LogoMediaId,
                    command.FaviconMediaId,
                    command.DefaultOpenGraphMediaId,
                    string.IsNullOrWhiteSpace(command.Status) ? existing.Status : command.Status,
                    command.ActorUserId
                );
            }
        }
        catch (ArgumentException)
        {
            return Result.Failure<Guid>(ContentErrors.InvalidSiteData);
        }

        var action = wasCreated
            ? ContentAuditActions.Created
            : ContentAuditActions.ResolveMutation(
                statusChanged: !string.Equals(previousStatus, existing.Status, StringComparison.Ordinal));
        await audit.PublishAsync(new ContentAuditEvent(
            wasCreated ? ContentAuditEventTypes.SiteCreated : ContentAuditEventTypes.SiteUpdated,
            action,
            ContentAuditEntityTypes.Site,
            existing.Id,
            command.ActorUserId,
            Before: before,
            After: Snapshot(existing)), cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(existing.Id);
    }

    private static object Snapshot(Site site) => new
    {
        site.Id,
        site.SiteKey,
        site.Name,
        site.PrimaryDomain,
        site.DefaultLocale,
        site.TimeZone,
        site.LogoMediaId,
        site.FaviconMediaId,
        site.DefaultOpenGraphMediaId,
        site.Status
    };
}
