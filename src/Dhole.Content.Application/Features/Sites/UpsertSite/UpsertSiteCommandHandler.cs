using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Content.Application.Abstractions.Repositories;
using Dhole.Content.Domain.Shared;
using Dhole.Content.Domain.Sites.Entities;

namespace Dhole.Content.Application.Sites.UpsertSite;

public sealed class UpsertSiteCommandHandler(
    ISiteRepository sites,
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

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(existing.Id);
    }
}
