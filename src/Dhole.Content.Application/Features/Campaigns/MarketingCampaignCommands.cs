using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Content.Application.Abstractions.Auditing;
using Dhole.Content.Application.Abstractions.Repositories;
using Dhole.Content.Application.Auditing;
using Dhole.Content.Domain.Campaigns.Entities;
using Dhole.Content.Domain.ContentItems.Enums;
using Dhole.Content.Domain.Shared;

namespace Dhole.Content.Application.Campaigns;

public sealed record CreateMarketingCampaignCommand(string SiteKey, string Name, string Slug, string Status,
    DateTime? StartsAtUtc, DateTime? EndsAtUtc, Guid? LandingContentId, string? UtmSource,
    string? UtmMedium, string? UtmCampaign, string GoalType, string? SettingsJson, Guid? ActorUserId)
    : ICommand<Result<Guid>>;
public sealed record UpdateMarketingCampaignCommand(Guid Id, string SiteKey, string Name, string Slug, string Status,
    DateTime? StartsAtUtc, DateTime? EndsAtUtc, Guid? LandingContentId, string? UtmSource,
    string? UtmMedium, string? UtmCampaign, string GoalType, string? SettingsJson, Guid? ActorUserId)
    : ICommand<Result>;
public sealed record DeleteMarketingCampaignCommand(Guid Id, Guid? ActorUserId) : ICommand<Result>;

internal static class MarketingCampaignCommandValidation
{
    public static async Task<Result> ValidateLandingAsync(string siteKey, Guid? landingContentId,
        IContentItemRepository contentItems, CancellationToken cancellationToken)
    {
        if (!landingContentId.HasValue) return Result.Success();
        var content = await contentItems.GetByIdAsync(landingContentId.Value, cancellationToken);
        if (content is null || content.IsDeleted) return Result.Failure(ContentErrors.ContentNotFound);
        if (!string.Equals(content.SiteKey, siteKey.Trim().ToLowerInvariant(), StringComparison.Ordinal) || content.Type != ContentType.Page)
            return Result.Failure(CampaignErrors.InvalidData);
        return Result.Success();
    }
}

public sealed class CreateMarketingCampaignCommandHandler(ISiteRepository sites, IContentItemRepository contentItems,
    IMarketingCampaignRepository campaigns, IContentAuditService audit, IUnitOfWork unitOfWork)
    : ICommandHandler<CreateMarketingCampaignCommand, Result<Guid>>
{
    public async Task<Result<Guid>> HandleAsync(CreateMarketingCampaignCommand command, CancellationToken cancellationToken = default)
    {
        var site = await sites.GetBySiteKeyAsync(command.SiteKey, cancellationToken);
        if (site is null || site.IsDeleted) return Result.Failure<Guid>(ContentErrors.SiteNotFound);
        if (await campaigns.ExistsBySlugAsync(command.SiteKey, command.Slug, null, cancellationToken))
            return Result.Failure<Guid>(CampaignErrors.SlugAlreadyExists);
        var landingValidation = await MarketingCampaignCommandValidation.ValidateLandingAsync(command.SiteKey,
            command.LandingContentId, contentItems, cancellationToken);
        if (landingValidation.IsFailure) return Result.Failure<Guid>(landingValidation.Error);

        MarketingCampaign campaign;
        try
        {
            campaign = MarketingCampaign.Create(command.SiteKey, command.Name, command.Slug, command.Status,
                command.StartsAtUtc, command.EndsAtUtc, command.LandingContentId, command.UtmSource,
                command.UtmMedium, command.UtmCampaign, command.GoalType, command.SettingsJson, command.ActorUserId);
        }
        catch (ArgumentException) { return Result.Failure<Guid>(CampaignErrors.InvalidData); }

        await campaigns.AddAsync(campaign, cancellationToken);
        await audit.PublishAsync(new ContentAuditEvent(ContentAuditEventTypes.MarketingCampaignCreated,
            ContentAuditActions.Created, ContentAuditEntityTypes.MarketingCampaign, campaign.Id,
            command.ActorUserId, After: Snapshot(campaign)), cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(campaign.Id);
    }

    internal static object Snapshot(MarketingCampaign campaign) => new
    {
        campaign.Id, campaign.SiteKey, campaign.Name, campaign.Slug, campaign.Status,
        campaign.StartsAtUtc, campaign.EndsAtUtc, campaign.LandingContentId,
        campaign.UtmSource, campaign.UtmMedium, campaign.UtmCampaign, campaign.GoalType
    };
}

public sealed class UpdateMarketingCampaignCommandHandler(ISiteRepository sites, IContentItemRepository contentItems,
    IMarketingCampaignRepository campaigns, IContentAuditService audit, IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateMarketingCampaignCommand, Result>
{
    public async Task<Result> HandleAsync(UpdateMarketingCampaignCommand command, CancellationToken cancellationToken = default)
    {
        var campaign = await campaigns.GetByIdAsync(command.Id, cancellationToken);
        if (campaign is null || campaign.IsDeleted) return Result.Failure(CampaignErrors.NotFound);
        var site = await sites.GetBySiteKeyAsync(command.SiteKey, cancellationToken);
        if (site is null || site.IsDeleted) return Result.Failure(ContentErrors.SiteNotFound);
        if (await campaigns.ExistsBySlugAsync(command.SiteKey, command.Slug, command.Id, cancellationToken))
            return Result.Failure(CampaignErrors.SlugAlreadyExists);
        var landingValidation = await MarketingCampaignCommandValidation.ValidateLandingAsync(command.SiteKey,
            command.LandingContentId, contentItems, cancellationToken);
        if (landingValidation.IsFailure) return landingValidation;
        var before = CreateMarketingCampaignCommandHandler.Snapshot(campaign);
        try
        {
            campaign.Update(command.SiteKey, command.Name, command.Slug, command.Status, command.StartsAtUtc,
                command.EndsAtUtc, command.LandingContentId, command.UtmSource, command.UtmMedium,
                command.UtmCampaign, command.GoalType, command.SettingsJson, command.ActorUserId);
        }
        catch (ArgumentException) { return Result.Failure(CampaignErrors.InvalidData); }

        await audit.PublishAsync(new ContentAuditEvent(ContentAuditEventTypes.MarketingCampaignUpdated,
            ContentAuditActions.Updated, ContentAuditEntityTypes.MarketingCampaign, campaign.Id,
            command.ActorUserId, Before: before, After: CreateMarketingCampaignCommandHandler.Snapshot(campaign)), cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public sealed class DeleteMarketingCampaignCommandHandler(IMarketingCampaignRepository campaigns,
    IContentAuditService audit, IUnitOfWork unitOfWork) : ICommandHandler<DeleteMarketingCampaignCommand, Result>
{
    public async Task<Result> HandleAsync(DeleteMarketingCampaignCommand command, CancellationToken cancellationToken = default)
    {
        var campaign = await campaigns.GetByIdAsync(command.Id, cancellationToken);
        if (campaign is null || campaign.IsDeleted) return Result.Failure(CampaignErrors.NotFound);
        var before = CreateMarketingCampaignCommandHandler.Snapshot(campaign);
        campaign.Delete(command.ActorUserId);
        await audit.PublishAsync(new ContentAuditEvent(ContentAuditEventTypes.MarketingCampaignDeleted,
            ContentAuditActions.Deleted, ContentAuditEntityTypes.MarketingCampaign, campaign.Id,
            command.ActorUserId, Before: before), cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
