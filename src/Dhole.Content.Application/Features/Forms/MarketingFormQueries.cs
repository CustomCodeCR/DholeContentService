using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Queries;
using Dhole.Content.Application.Abstractions.Repositories;
using Dhole.Content.Contracts.Forms;
using Dhole.Content.Domain.Forms;
using Dhole.Content.Domain.Forms.Entities;
using Dhole.Content.Domain.Shared;

namespace Dhole.Content.Application.Forms;

public sealed record GetMarketingFormsQuery(string? SiteKey, string? Purpose, string? Status) : IQuery<IReadOnlyCollection<MarketingFormDto>>;
public sealed record GetMarketingFormByIdQuery(Guid Id) : IQuery<Result<MarketingFormDto>>;
public sealed record GetMarketingFormFieldsQuery(Guid FormId) : IQuery<Result<IReadOnlyCollection<MarketingFormFieldDto>>>;
public sealed record GetMarketingFormPurposesQuery() : IQuery<IReadOnlyCollection<MarketingFormPurposeDto>>;

public sealed class GetMarketingFormsQueryHandler(IMarketingFormRepository forms)
    : IQueryHandler<GetMarketingFormsQuery, IReadOnlyCollection<MarketingFormDto>>
{
    public async Task<IReadOnlyCollection<MarketingFormDto>> HandleAsync(GetMarketingFormsQuery query, CancellationToken cancellationToken = default)
        => (await forms.GetAllAsync(query.SiteKey, query.Purpose, query.Status, cancellationToken)).Select(Map).ToArray();

    internal static MarketingFormDto Map(MarketingForm form)
        => new(form.Id, form.SiteKey, form.FormKey, form.Name, form.Purpose, form.Status, form.SuccessMessage,
            form.NotificationTemplateKey, form.SettingsJson, form.Version, form.CreatedAtUtc, form.UpdatedAtUtc);
}

public sealed class GetMarketingFormByIdQueryHandler(IMarketingFormRepository forms)
    : IQueryHandler<GetMarketingFormByIdQuery, Result<MarketingFormDto>>
{
    public async Task<Result<MarketingFormDto>> HandleAsync(GetMarketingFormByIdQuery query, CancellationToken cancellationToken = default)
    {
        var form = await forms.GetByIdAsync(query.Id, cancellationToken);
        return form is null || form.IsDeleted
            ? Result.Failure<MarketingFormDto>(ContentErrors.MarketingFormNotFound)
            : Result.Success(GetMarketingFormsQueryHandler.Map(form));
    }
}

public sealed class GetMarketingFormFieldsQueryHandler(
    IMarketingFormRepository forms,
    IMarketingFormFieldRepository fields)
    : IQueryHandler<GetMarketingFormFieldsQuery, Result<IReadOnlyCollection<MarketingFormFieldDto>>>
{
    public async Task<Result<IReadOnlyCollection<MarketingFormFieldDto>>> HandleAsync(GetMarketingFormFieldsQuery query, CancellationToken cancellationToken = default)
    {
        var form = await forms.GetByIdAsync(query.FormId, cancellationToken);
        if (form is null || form.IsDeleted)
            return Result.Failure<IReadOnlyCollection<MarketingFormFieldDto>>(ContentErrors.MarketingFormNotFound);

        var result = (await fields.GetByFormAsync(query.FormId, cancellationToken))
            .Select(field => new MarketingFormFieldDto(field.Id, field.FormId, field.FieldKey, field.Label,
                field.FieldType, field.Placeholder, field.IsRequired, field.SortOrder, field.ValidationJson,
                field.OptionsJson, field.CreatedAtUtc, field.UpdatedAtUtc))
            .ToArray();
        return Result.Success<IReadOnlyCollection<MarketingFormFieldDto>>(result);
    }
}

public sealed class GetMarketingFormPurposesQueryHandler
    : IQueryHandler<GetMarketingFormPurposesQuery, IReadOnlyCollection<MarketingFormPurposeDto>>
{
    private static readonly IReadOnlyCollection<MarketingFormPurposeDto> Purposes =
    [
        new(MarketingFormRules.PurposeContact, "Contacto"),
        new(MarketingFormRules.PurposeQuoteRequest, "Solicitud de cotización"),
        new(MarketingFormRules.PurposeMeeting, "Agendar reunión"),
        new(MarketingFormRules.PurposeNewsletter, "Newsletter"),
        new(MarketingFormRules.PurposeCampaign, "Campaña")
    ];

    public Task<IReadOnlyCollection<MarketingFormPurposeDto>> HandleAsync(GetMarketingFormPurposesQuery query, CancellationToken cancellationToken = default)
        => Task.FromResult(Purposes);
}
