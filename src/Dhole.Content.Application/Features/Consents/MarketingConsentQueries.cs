using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Queries;
using Dhole.Content.Application.Abstractions.Repositories;
using Dhole.Content.Contracts.Consents;
using Dhole.Content.Domain.Consents;
using Dhole.Content.Domain.Consents.Entities;
using Dhole.Content.Domain.Shared;

namespace Dhole.Content.Application.Consents;

public sealed record GetMarketingConsentsQuery(
    Guid? LeadId,
    Guid? SubmissionId,
    string? Purpose,
    bool? Granted,
    DateTime? CapturedFromUtc,
    DateTime? CapturedToUtc) : IQuery<IReadOnlyCollection<MarketingConsentDto>>;

public sealed record GetMarketingConsentByIdQuery(Guid Id) : IQuery<Result<MarketingConsentDto>>;
public sealed record GetMarketingConsentPurposesQuery() : IQuery<IReadOnlyCollection<MarketingConsentPurposeDto>>;

public sealed class GetMarketingConsentsQueryHandler(IMarketingConsentRepository consents)
    : IQueryHandler<GetMarketingConsentsQuery, IReadOnlyCollection<MarketingConsentDto>>
{
    public async Task<IReadOnlyCollection<MarketingConsentDto>> HandleAsync(
        GetMarketingConsentsQuery query,
        CancellationToken cancellationToken = default)
        => (await consents.GetAllAsync(query.LeadId, query.SubmissionId, query.Purpose, query.Granted,
                query.CapturedFromUtc, query.CapturedToUtc, cancellationToken))
            .Select(Map)
            .ToArray();

    internal static MarketingConsentDto Map(MarketingConsent consent)
        => new(consent.Id, consent.LeadId, consent.SubmissionId, consent.Purpose, consent.Granted,
            consent.PolicyVersion, consent.Source, consent.CapturedAtUtc, consent.CreatedAtUtc);
}

public sealed class GetMarketingConsentByIdQueryHandler(IMarketingConsentRepository consents)
    : IQueryHandler<GetMarketingConsentByIdQuery, Result<MarketingConsentDto>>
{
    public async Task<Result<MarketingConsentDto>> HandleAsync(
        GetMarketingConsentByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var consent = await consents.GetByIdAsync(query.Id, cancellationToken);
        return consent is null || consent.IsDeleted
            ? Result.Failure<MarketingConsentDto>(ContentErrors.MarketingConsentNotFound)
            : Result.Success(GetMarketingConsentsQueryHandler.Map(consent));
    }
}

public sealed class GetMarketingConsentPurposesQueryHandler
    : IQueryHandler<GetMarketingConsentPurposesQuery, IReadOnlyCollection<MarketingConsentPurposeDto>>
{
    private static readonly IReadOnlyCollection<MarketingConsentPurposeDto> Purposes =
    [
        new(MarketingConsentRules.PurposePrivacy, "Privacidad"),
        new(MarketingConsentRules.PurposeContact, "Contacto"),
        new(MarketingConsentRules.PurposeNewsletter, "Newsletter"),
        new(MarketingConsentRules.PurposeCommercialCommunications, "Comunicaciones comerciales")
    ];

    public Task<IReadOnlyCollection<MarketingConsentPurposeDto>> HandleAsync(
        GetMarketingConsentPurposesQuery query,
        CancellationToken cancellationToken = default)
        => Task.FromResult(Purposes);
}
