using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Queries;
using Dhole.Content.Application.Abstractions.Repositories;
using Dhole.Content.Contracts.Leads;
using Dhole.Content.Domain.Leads.Entities;
using Dhole.Content.Domain.Shared;

namespace Dhole.Content.Application.Leads;

public sealed record GetMarketingLeadsQuery(string? SiteKey, string? Status, Guid? OwnerUserId, string? Source)
    : IQuery<IReadOnlyCollection<MarketingLeadDto>>;
public sealed record GetMarketingLeadByIdQuery(Guid Id) : IQuery<Result<MarketingLeadDto>>;

public sealed class GetMarketingLeadsQueryHandler(IMarketingLeadRepository leads)
    : IQueryHandler<GetMarketingLeadsQuery, IReadOnlyCollection<MarketingLeadDto>>
{
    public async Task<IReadOnlyCollection<MarketingLeadDto>> HandleAsync(GetMarketingLeadsQuery query, CancellationToken cancellationToken = default)
        => (await leads.GetAllAsync(query.SiteKey, query.Status, query.OwnerUserId, query.Source, cancellationToken))
            .Select(Map)
            .ToArray();

    internal static MarketingLeadDto Map(MarketingLead lead)
        => new(lead.Id, lead.SiteKey, lead.FirstName, lead.LastName, lead.Email, lead.Phone, lead.Company,
            lead.JobTitle, lead.Country, lead.Source, lead.Status, lead.OwnerUserId, lead.FirstTouchAtUtc,
            lead.LastTouchAtUtc, lead.CreatedAtUtc, lead.UpdatedAtUtc);
}

public sealed class GetMarketingLeadByIdQueryHandler(IMarketingLeadRepository leads)
    : IQueryHandler<GetMarketingLeadByIdQuery, Result<MarketingLeadDto>>
{
    public async Task<Result<MarketingLeadDto>> HandleAsync(GetMarketingLeadByIdQuery query, CancellationToken cancellationToken = default)
    {
        var lead = await leads.GetByIdAsync(query.Id, cancellationToken);
        return lead is null || lead.IsDeleted
            ? Result.Failure<MarketingLeadDto>(ContentErrors.MarketingLeadNotFound)
            : Result.Success(GetMarketingLeadsQueryHandler.Map(lead));
    }
}
