using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Content.Application.Abstractions.Auditing;
using Dhole.Content.Application.Abstractions.Repositories;
using Dhole.Content.Application.Auditing;
using Dhole.Content.Domain.Redirects.Entities;
using Dhole.Content.Domain.Shared;

namespace Dhole.Content.Application.Redirects;

public sealed record CreateRedirectCommand(string SiteKey, string SourcePath, string TargetUrl, int StatusCode,
    bool IsActive, DateTime? ValidFromUtc, DateTime? ValidToUtc, Guid? ActorUserId) : ICommand<Result<Guid>>;
public sealed record UpdateRedirectCommand(Guid Id, string SiteKey, string SourcePath, string TargetUrl, int StatusCode,
    bool IsActive, DateTime? ValidFromUtc, DateTime? ValidToUtc, Guid? ActorUserId) : ICommand<Result>;
public sealed record DeleteRedirectCommand(Guid Id, Guid? ActorUserId) : ICommand<Result>;

public static class RedirectAuditSnapshot
{
    public static object From(ContentRedirect redirect) => new
    {
        redirect.Id, redirect.SiteKey, redirect.SourcePath, redirect.TargetUrl, redirect.StatusCode,
        redirect.IsActive, redirect.ValidFromUtc, redirect.ValidToUtc
    };
}

public sealed class CreateRedirectCommandHandler(
    ISiteRepository sites,
    IRedirectRepository redirects,
    IContentAuditService audit,
    IUnitOfWork unitOfWork) : ICommandHandler<CreateRedirectCommand, Result<Guid>>
{
    public async Task<Result<Guid>> HandleAsync(CreateRedirectCommand command, CancellationToken cancellationToken = default)
    {
        var site = await sites.GetBySiteKeyAsync(command.SiteKey, cancellationToken);
        if (site is null || site.IsDeleted) return Result.Failure<Guid>(ContentErrors.SiteNotFound);
        if (await redirects.ExistsSourcePathAsync(command.SiteKey, command.SourcePath, null, cancellationToken))
            return Result.Failure<Guid>(ContentErrors.RedirectSourceAlreadyExists);

        ContentRedirect redirect;
        try
        {
            redirect = ContentRedirect.Create(command.SiteKey, command.SourcePath, command.TargetUrl, command.StatusCode,
                command.IsActive, command.ValidFromUtc, command.ValidToUtc, command.ActorUserId);
        }
        catch (ArgumentException)
        {
            return Result.Failure<Guid>(ContentErrors.InvalidRedirectData);
        }

        await redirects.AddAsync(redirect, cancellationToken);
        await audit.PublishAsync(new ContentAuditEvent(ContentAuditEventTypes.RedirectCreated, ContentAuditActions.Created,
            ContentAuditEntityTypes.Redirect, redirect.Id, command.ActorUserId, After: RedirectAuditSnapshot.From(redirect)), cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(redirect.Id);
    }
}

public sealed class UpdateRedirectCommandHandler(
    ISiteRepository sites,
    IRedirectRepository redirects,
    IContentAuditService audit,
    IUnitOfWork unitOfWork) : ICommandHandler<UpdateRedirectCommand, Result>
{
    public async Task<Result> HandleAsync(UpdateRedirectCommand command, CancellationToken cancellationToken = default)
    {
        var redirect = await redirects.GetByIdAsync(command.Id, cancellationToken);
        if (redirect is null || redirect.IsDeleted) return Result.Failure(ContentErrors.RedirectNotFound);
        var site = await sites.GetBySiteKeyAsync(command.SiteKey, cancellationToken);
        if (site is null || site.IsDeleted) return Result.Failure(ContentErrors.SiteNotFound);
        if (await redirects.ExistsSourcePathAsync(command.SiteKey, command.SourcePath, command.Id, cancellationToken))
            return Result.Failure(ContentErrors.RedirectSourceAlreadyExists);

        var before = RedirectAuditSnapshot.From(redirect);
        try
        {
            redirect.Update(command.SiteKey, command.SourcePath, command.TargetUrl, command.StatusCode,
                command.IsActive, command.ValidFromUtc, command.ValidToUtc, command.ActorUserId);
        }
        catch (ArgumentException)
        {
            return Result.Failure(ContentErrors.InvalidRedirectData);
        }

        await audit.PublishAsync(new ContentAuditEvent(ContentAuditEventTypes.RedirectUpdated, ContentAuditActions.Updated,
            ContentAuditEntityTypes.Redirect, redirect.Id, command.ActorUserId, Before: before,
            After: RedirectAuditSnapshot.From(redirect)), cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public sealed class DeleteRedirectCommandHandler(
    IRedirectRepository redirects,
    IContentAuditService audit,
    IUnitOfWork unitOfWork) : ICommandHandler<DeleteRedirectCommand, Result>
{
    public async Task<Result> HandleAsync(DeleteRedirectCommand command, CancellationToken cancellationToken = default)
    {
        var redirect = await redirects.GetByIdAsync(command.Id, cancellationToken);
        if (redirect is null || redirect.IsDeleted) return Result.Failure(ContentErrors.RedirectNotFound);
        var before = RedirectAuditSnapshot.From(redirect);
        redirect.Delete(command.ActorUserId);
        await audit.PublishAsync(new ContentAuditEvent(ContentAuditEventTypes.RedirectDeleted, ContentAuditActions.Deleted,
            ContentAuditEntityTypes.Redirect, redirect.Id, command.ActorUserId, Before: before), cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
