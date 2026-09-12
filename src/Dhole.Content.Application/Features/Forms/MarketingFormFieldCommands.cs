using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Content.Application.Abstractions.Auditing;
using Dhole.Content.Application.Abstractions.Repositories;
using Dhole.Content.Application.Auditing;
using Dhole.Content.Domain.Forms.Entities;
using Dhole.Content.Domain.Shared;

namespace Dhole.Content.Application.Forms;

public sealed record CreateMarketingFormFieldCommand(
    Guid FormId,
    string FieldKey,
    string Label,
    string FieldType,
    string? Placeholder,
    bool IsRequired,
    int SortOrder,
    string? ValidationJson,
    string? OptionsJson,
    Guid? ActorUserId) : ICommand<Result<Guid>>;

public sealed record UpdateMarketingFormFieldCommand(
    Guid FormId,
    Guid FieldId,
    string FieldKey,
    string Label,
    string FieldType,
    string? Placeholder,
    bool IsRequired,
    int SortOrder,
    string? ValidationJson,
    string? OptionsJson,
    Guid? ActorUserId) : ICommand<Result>;

public sealed record DeleteMarketingFormFieldCommand(Guid FormId, Guid FieldId, Guid? ActorUserId) : ICommand<Result>;

public sealed class CreateMarketingFormFieldCommandHandler(
    IMarketingFormRepository forms,
    IMarketingFormFieldRepository fields,
    IContentAuditService audit,
    IUnitOfWork unitOfWork) : ICommandHandler<CreateMarketingFormFieldCommand, Result<Guid>>
{
    public async Task<Result<Guid>> HandleAsync(CreateMarketingFormFieldCommand command, CancellationToken cancellationToken = default)
    {
        var form = await forms.GetByIdAsync(command.FormId, cancellationToken);
        if (form is null || form.IsDeleted) return Result.Failure<Guid>(ContentErrors.MarketingFormNotFound);
        if (await fields.ExistsByFieldKeyAsync(command.FormId, command.FieldKey, null, cancellationToken))
            return Result.Failure<Guid>(ContentErrors.MarketingFormFieldKeyAlreadyExists);

        MarketingFormField field;
        try
        {
            field = MarketingFormField.Create(command.FormId, command.FieldKey, command.Label, command.FieldType,
                command.Placeholder, command.IsRequired, command.SortOrder, command.ValidationJson,
                command.OptionsJson, command.ActorUserId);
        }
        catch (ArgumentException)
        {
            return Result.Failure<Guid>(ContentErrors.InvalidMarketingFormData);
        }

        form.TouchFieldChange(command.ActorUserId);
        await fields.AddAsync(field, cancellationToken);
        await audit.PublishAsync(new ContentAuditEvent(ContentAuditEventTypes.MarketingFormFieldCreated, ContentAuditActions.Created,
            ContentAuditEntityTypes.MarketingFormField, field.Id, command.ActorUserId, After: Snapshot(field),
            Payload: new { form.Id, form.Version }), cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(field.Id);
    }

    internal static object Snapshot(MarketingFormField field) => new
    {
        field.Id,
        field.FormId,
        field.FieldKey,
        field.Label,
        field.FieldType,
        field.Placeholder,
        field.IsRequired,
        field.SortOrder,
        field.ValidationJson,
        field.OptionsJson
    };
}

public sealed class UpdateMarketingFormFieldCommandHandler(
    IMarketingFormRepository forms,
    IMarketingFormFieldRepository fields,
    IContentAuditService audit,
    IUnitOfWork unitOfWork) : ICommandHandler<UpdateMarketingFormFieldCommand, Result>
{
    public async Task<Result> HandleAsync(UpdateMarketingFormFieldCommand command, CancellationToken cancellationToken = default)
    {
        var form = await forms.GetByIdAsync(command.FormId, cancellationToken);
        if (form is null || form.IsDeleted) return Result.Failure(ContentErrors.MarketingFormNotFound);
        var field = await fields.GetByIdAsync(command.FieldId, cancellationToken);
        if (field is null || field.IsDeleted || field.FormId != command.FormId)
            return Result.Failure(ContentErrors.MarketingFormFieldNotFound);
        if (await fields.ExistsByFieldKeyAsync(command.FormId, command.FieldKey, command.FieldId, cancellationToken))
            return Result.Failure(ContentErrors.MarketingFormFieldKeyAlreadyExists);

        var before = CreateMarketingFormFieldCommandHandler.Snapshot(field);
        try
        {
            field.Update(command.FieldKey, command.Label, command.FieldType, command.Placeholder,
                command.IsRequired, command.SortOrder, command.ValidationJson, command.OptionsJson, command.ActorUserId);
        }
        catch (ArgumentException)
        {
            return Result.Failure(ContentErrors.InvalidMarketingFormData);
        }

        form.TouchFieldChange(command.ActorUserId);
        await audit.PublishAsync(new ContentAuditEvent(ContentAuditEventTypes.MarketingFormFieldUpdated, ContentAuditActions.Updated,
            ContentAuditEntityTypes.MarketingFormField, field.Id, command.ActorUserId, Before: before,
            After: CreateMarketingFormFieldCommandHandler.Snapshot(field), Payload: new { form.Id, form.Version }), cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public sealed class DeleteMarketingFormFieldCommandHandler(
    IMarketingFormRepository forms,
    IMarketingFormFieldRepository fields,
    IContentAuditService audit,
    IUnitOfWork unitOfWork) : ICommandHandler<DeleteMarketingFormFieldCommand, Result>
{
    public async Task<Result> HandleAsync(DeleteMarketingFormFieldCommand command, CancellationToken cancellationToken = default)
    {
        var form = await forms.GetByIdAsync(command.FormId, cancellationToken);
        if (form is null || form.IsDeleted) return Result.Failure(ContentErrors.MarketingFormNotFound);
        var field = await fields.GetByIdAsync(command.FieldId, cancellationToken);
        if (field is null || field.IsDeleted || field.FormId != command.FormId)
            return Result.Failure(ContentErrors.MarketingFormFieldNotFound);

        var before = CreateMarketingFormFieldCommandHandler.Snapshot(field);
        field.Delete(command.ActorUserId);
        form.TouchFieldChange(command.ActorUserId);
        await audit.PublishAsync(new ContentAuditEvent(ContentAuditEventTypes.MarketingFormFieldDeleted, ContentAuditActions.Deleted,
            ContentAuditEntityTypes.MarketingFormField, field.Id, command.ActorUserId, Before: before,
            Payload: new { form.Id, form.Version }), cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
