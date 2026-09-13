using CustomCodeFramework.Cqrs.Dispatching;
using Dhole.Content.Application.Analytics;

namespace Dhole.Content.Api.Endpoints;

public static class PublicAnalyticsEndpoints
{
    public static IEndpointRouteBuilder MapPublicAnalyticsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/public/analytics")
            .WithTags("Public Analytics")
            .AllowAnonymous();

        group.MapPost("/page-views", async (TrackPageViewRequest request, ICommandDispatcher dispatcher, HttpContext context, CancellationToken ct) =>
            EndpointResults.FromResult(await dispatcher.DispatchAsync(new TrackPageViewCommand(
                request.SiteKey ?? "main", request.ContentId, request.Path, request.Locale ?? "es-CR", request.CampaignId,
                request.ReferrerUrl, request.UtmSource, request.UtmMedium, request.UtmCampaign, request.UtmContent, request.UtmTerm), ct), context));

        group.MapPost("/clicks", async (TrackInteractionClickRequest request, ICommandDispatcher dispatcher, HttpContext context, CancellationToken ct) =>
            EndpointResults.FromResult(await dispatcher.DispatchAsync(new TrackInteractionClickCommand(
                request.SiteKey ?? "main", request.ContentId, request.PlacementId, request.CampaignId,
                request.InteractionType, request.TargetKey, request.SourceUrl,
                request.UtmSource, request.UtmMedium, request.UtmCampaign, request.UtmContent, request.UtmTerm), ct), context));

        return app;
    }

    public sealed record TrackPageViewRequest(
        string? SiteKey,
        Guid? ContentId,
        string Path,
        string? Locale,
        Guid? CampaignId,
        string? ReferrerUrl,
        string? UtmSource,
        string? UtmMedium,
        string? UtmCampaign,
        string? UtmContent,
        string? UtmTerm);

    public sealed record TrackInteractionClickRequest(
        string? SiteKey,
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
        string? UtmTerm);
}
