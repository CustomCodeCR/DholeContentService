using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Queries;
using Dhole.Content.Application.Abstractions.Repositories;
using Dhole.Content.Contracts.Campaigns;
using Dhole.Content.Domain.Campaigns.Entities;
using Dhole.Content.Domain.Shared;

namespace Dhole.Content.Application.Campaigns;

public sealed record GetMarketingCampaignsQuery(string? SiteKey, string? Status)
    : IQuery<IReadOnlyCollection<MarketingCampaignDto>>;
public sealed record GetMarketingCampaignByIdQuery(Guid Id) : IQuery<Result<MarketingCampaignDto>>;

public sealed class GetMarketingCampaignsQueryHandler(IMarketingCampaignRepository campaigns)
    : IQueryHandler<GetMarketingCampaignsQuery, IReadOnlyCollection<MarketingCampaignDto>>
{
    public async Task<IReadOnlyCollection<MarketingCampaignDto>> HandleAsync(GetMarketingCampaignsQuery query, CancellationToken cancellationToken = default)
        => (await campaigns.GetAllAsync(query.SiteKey, query.Status, cancellationToken)).Select(Map).ToArray();

    internal static MarketingCampaignDto Map(MarketingCampaign campaign)
        => new(campaign.Id, campaign.SiteKey, campaign.Name, campaign.Slug, campaign.Status,
            campaign.StartsAtUtc, campaign.EndsAtUtc, campaign.LandingContentId, campaign.UtmSource,
            campaign.UtmMedium, campaign.UtmCampaign, campaign.GoalType, campaign.SettingsJson,
            campaign.CreatedAtUtc, campaign.UpdatedAtUtc);
}

public sealed class GetMarketingCampaignByIdQueryHandler(IMarketingCampaignRepository campaigns)
    : IQueryHandler<GetMarketingCampaignByIdQuery, Result<MarketingCampaignDto>>
{
    public async Task<Result<MarketingCampaignDto>> HandleAsync(GetMarketingCampaignByIdQuery query, CancellationToken cancellationToken = default)
    {
        var campaign = await campaigns.GetByIdAsync(query.Id, cancellationToken);
        return campaign is null || campaign.IsDeleted
            ? Result.Failure<MarketingCampaignDto>(ContentErrors.MarketingCampaignNotFound)
            : Result.Success(GetMarketingCampaignsQueryHandler.Map(campaign));
    }
}
