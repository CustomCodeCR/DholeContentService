using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Content.Application.Abstractions.Messaging;

namespace Dhole.Content.Application.Analytics;

public static class ContentAnalyticsIntegration
{
    public const string PageViewedEventName = "content.analytics.page-viewed";
    public const string PageViewedEventType = "Content.Analytics.PageViewed";
    public const string FormSubmittedEventName = "content.analytics.form-submitted";
    public const string FormSubmittedEventType = "Content.Analytics.FormSubmitted";
    public const string InteractionClickedEventName = "content.analytics.interaction-clicked";
    public const string InteractionClickedEventType = "Content.Analytics.InteractionClicked";
}

public sealed record PageViewedAnalyticsEvent(
    Guid AnalyticsEventId,
    string SiteKey,
    Guid? ContentId,
    string Path,
    string Locale,
    Guid? CampaignId,
    string? ReferrerUrl,
    string? UtmSource,
    string? UtmMedium,
    string? UtmCampaign,
    string? UtmContent,
    string? UtmTerm,
    DateTime OccurredAtUtc);

public sealed record FormSubmittedAnalyticsEvent(
    Guid AnalyticsEventId,
    string SiteKey,
    Guid SubmissionId,
    Guid FormId,
    Guid? ContentId,
    Guid? CampaignId,
    string SourceUrl,
    string? ReferrerUrl,
    string? UtmSource,
    string? UtmMedium,
    string? UtmCampaign,
    string? UtmContent,
    string? UtmTerm,
    DateTime OccurredAtUtc);

public sealed record InteractionClickedAnalyticsEvent(
    Guid AnalyticsEventId,
    string SiteKey,
    Guid? ContentId,
    Guid? PlacementId,
    Guid? CampaignId,
    string InteractionType,
    string TargetKey,
    string? SourceUrl,
    string? UtmSource,
    string? UtmMedium,
    string? UtmCampaign,
    string? UtmContent,
    string? UtmTerm,
    DateTime OccurredAtUtc);

public sealed record TrackPageViewCommand(
    string SiteKey,
    Guid? ContentId,
    string Path,
    string Locale,
    Guid? CampaignId,
    string? ReferrerUrl,
    string? UtmSource,
    string? UtmMedium,
    string? UtmCampaign,
    string? UtmContent,
    string? UtmTerm) : ICommand<Result<Guid>>;

public sealed record TrackInteractionClickCommand(
    string SiteKey,
    Guid? ContentId,
    Guid? PlacementId,
    Guid? CampaignId,
    string InteractionType,
    string TargetKey,
    string? SourceUrl,
    string? UtmSource,
    string? UtmMedium,
    string? UtmCampaign,
    string? UtmContent,
    string? UtmTerm) : ICommand<Result<Guid>>;

public sealed class TrackPageViewCommandHandler(IIntegrationEventOutboxWriter outbox, IUnitOfWork unitOfWork)
    : ICommandHandler<TrackPageViewCommand, Result<Guid>>
{
    public async Task<Result<Guid>> HandleAsync(TrackPageViewCommand command, CancellationToken ct = default)
    {
        var eventId = Guid.NewGuid();
        var payload = new PageViewedAnalyticsEvent(
            eventId,
            Normalize(command.SiteKey, "main", 64),
            command.ContentId,
            NormalizePath(command.Path),
            Normalize(command.Locale, "es-CR", 16),
            command.CampaignId,
            Trim(command.ReferrerUrl, 1024),
            Trim(command.UtmSource, 200),
            Trim(command.UtmMedium, 200),
            Trim(command.UtmCampaign, 200),
            Trim(command.UtmContent, 200),
            Trim(command.UtmTerm, 200),
            DateTime.UtcNow);
        await outbox.WriteAsync(ContentAnalyticsIntegration.PageViewedEventName, ContentAnalyticsIntegration.PageViewedEventType, payload, cancellationToken: ct);
        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success(eventId);
    }

    private static string NormalizePath(string? value)
    {
        var path = Normalize(value, "/", 1024);
        return path.StartsWith('/') ? path : $"/{path}";
    }

    internal static string Normalize(string? value, string fallback, int maxLength)
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
        return normalized.Length <= maxLength ? normalized : normalized[..maxLength];
    }

    internal static string? Trim(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var normalized = value.Trim();
        return normalized.Length <= maxLength ? normalized : normalized[..maxLength];
    }
}

public sealed class TrackInteractionClickCommandHandler(IIntegrationEventOutboxWriter outbox, IUnitOfWork unitOfWork)
    : ICommandHandler<TrackInteractionClickCommand, Result<Guid>>
{
    public async Task<Result<Guid>> HandleAsync(TrackInteractionClickCommand command, CancellationToken ct = default)
    {
        var eventId = Guid.NewGuid();
        var interactionType = TrackPageViewCommandHandler.Normalize(command.InteractionType, "cta", 32).ToLowerInvariant();
        var payload = new InteractionClickedAnalyticsEvent(
            eventId,
            TrackPageViewCommandHandler.Normalize(command.SiteKey, "main", 64),
            command.ContentId,
            command.PlacementId,
            command.CampaignId,
            interactionType,
            TrackPageViewCommandHandler.Normalize(command.TargetKey, "unknown", 200),
            TrackPageViewCommandHandler.Trim(command.SourceUrl, 1024),
            TrackPageViewCommandHandler.Trim(command.UtmSource, 200),
            TrackPageViewCommandHandler.Trim(command.UtmMedium, 200),
            TrackPageViewCommandHandler.Trim(command.UtmCampaign, 200),
            TrackPageViewCommandHandler.Trim(command.UtmContent, 200),
            TrackPageViewCommandHandler.Trim(command.UtmTerm, 200),
            DateTime.UtcNow);
        await outbox.WriteAsync(ContentAnalyticsIntegration.InteractionClickedEventName, ContentAnalyticsIntegration.InteractionClickedEventType, payload, cancellationToken: ct);
        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success(eventId);
    }
}
