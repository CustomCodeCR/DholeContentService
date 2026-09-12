using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Content.Application.Abstractions.Auditing;
using Dhole.Content.Application.Abstractions.Repositories;
using Dhole.Content.Application.Auditing;
using Dhole.Content.Domain.Forms.Entities;
using Dhole.Content.Domain.Shared;

namespace Dhole.Content.Application.Forms;

public sealed record CreateMarketingFormCommand(
    string SiteKey,
    string FormKey,
    string Name,
    string Purpose,
    string Status,
    string? SuccessMessage,
    string? NotificationTemplateKey,
    string? SettingsJson,
    Guid? ActorUserId) : ICommand<Result<Guid>>;

public sealed record UpdateMarketingFormCommand(
    Guid Id,
    string SiteKey,
    string FormKey,
    string Name,
    string Purpose,
    string Status,
    string? SuccessMessage,
    string? NotificationTemplateKey,
    string? SettingsJson,
    Guid? ActorUserId) : ICommand<Result>;

public sealed record DeleteMarketingFormCommand(Guid Id, Guid? ActorUserId) : ICommand<Result>;

public sealed class CreateMarketingFormCommandHandler(
    ISiteRepository sites,
    IMarketingFormRepository forms,
    IContentAuditService audit,
    IUnitOfWork unitOfWork) : ICommandHandler<CreateMarketingFormCommand, Result<Guid>>
{
    public async Task<Result<Guid>> HandleAsync(CreateMarketingFormCommand command, CancellationToken cancellationToken = default)
    {
        var site = await sites.GetBySiteKeyAsync(command.SiteKey, cancellationToken);
        if (site is null || site.IsDeleted) return Result.Failure<Guid>(ContentErrors.SiteNotFound);
        if (await forms.ExistsByFormKeyAsync(command.SiteKey, command.FormKey, null, cancellationToken))
            return Result.Failure<Guid>(ContentErrors.MarketingFormKeyAlreadyExists);

        MarketingForm form;
        try
        {
            form = MarketingForm.Create(command.SiteKey, command.FormKey, command.Name, command.Purpose, command.Status,
                command.SuccessMessage, command.NotificationTemplateKey, command.SettingsJson, command.ActorUserId);
        }
        catch (ArgumentException)
        {
            return Result.Failure<Guid>(ContentErrors.InvalidMarketingFormData);
        }

        await forms.AddAsync(form, cancellationToken);
        await audit.PublishAsync(new ContentAuditEvent(ContentAuditEventTypes.MarketingFormCreated, ContentAuditActions.Created,
            ContentAuditEntityTypes.MarketingForm, form.Id, command.ActorUserId, After: Snapshot(form)), cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(form.Id);
    }

    internal static object Snapshot(MarketingForm form) => new
    {
        form.Id,
        form.SiteKey,
        form.FormKey,
        form.Name,
        form.Purpose,
        form.Status,
        form.SuccessMessage,
        form.NotificationTemplateKey,
        form.SettingsJson,
        form.Version
    };
}

public sealed class UpdateMarketingFormCommandHandler(
    ISiteRepository sites,
    IMarketingFormRepository forms,
    IContentAuditService audit,
    IUnitOfWork unitOfWork) : ICommandHandler<UpdateMarketingFormCommand, Result>
{
    public async Task<Result> HandleAsync(UpdateMarketingFormCommand command, CancellationToken cancellationToken = default)
    {
        var form = await forms.GetByIdAsync(command.Id, cancellationToken);
        if (form is null || form.IsDeleted) return Result.Failure(ContentErrors.MarketingFormNotFound);
        var site = await sites.GetBySiteKeyAsync(command.SiteKey, cancellationToken);
        if (site is null || site.IsDeleted) return Result.Failure(ContentErrors.SiteNotFound);
        if (await forms.ExistsByFormKeyAsync(command.SiteKey, command.FormKey, command.Id, cancellationToken))
            return Result.Failure(ContentErrors.MarketingFormKeyAlreadyExists);

        var before = CreateMarketingFormCommandHandler.Snapshot(form);
        try
        {
            form.Update(command.SiteKey, command.FormKey, command.Name, command.Purpose, command.Status,
                command.SuccessMessage, command.NotificationTemplateKey, command.SettingsJson, command.ActorUserId);
        }
        catch (ArgumentException)
        {
            return Result.Failure(ContentErrors.InvalidMarketingFormData);
        }

        await audit.PublishAsync(new ContentAuditEvent(ContentAuditEventTypes.MarketingFormUpdated, ContentAuditActions.Updated,
            ContentAuditEntityTypes.MarketingForm, form.Id, command.ActorUserId, Before: before,
            After: CreateMarketingFormCommandHandler.Snapshot(form)), cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public sealed class DeleteMarketingFormCommandHandler(
    IMarketingFormRepository forms,
    IContentAuditService audit,
    IUnitOfWork unitOfWork) : ICommandHandler<DeleteMarketingFormCommand, Result>
{
    public async Task<Result> HandleAsync(DeleteMarketingFormCommand command, CancellationToken cancellationToken = default)
    {
        var form = await forms.GetByIdAsync(command.Id, cancellationToken);
        if (form is null || form.IsDeleted) return Result.Failure(ContentErrors.MarketingFormNotFound);

        var before = CreateMarketingFormCommandHandler.Snapshot(form);
        form.Delete(command.ActorUserId);
        await audit.PublishAsync(new ContentAuditEvent(ContentAuditEventTypes.MarketingFormDeleted, ContentAuditActions.Deleted,
            ContentAuditEntityTypes.MarketingForm, form.Id, command.ActorUserId, Before: before), cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
