using CustomCodeFramework.Cqrs.Dispatching;
using Dhole.Content.Api.Authorization;
using Dhole.Content.Api.Extensions;
using Dhole.Content.Application.Campaigns;
using Dhole.Content.Contracts.Campaigns;

namespace Dhole.Content.Api.Endpoints;

public static class MarketingCampaignEndpoints
{
    public static IEndpointRouteBuilder MapMarketingCampaignEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/content/campaigns").WithTags("Marketing Campaigns").RequireAuthorization();
        group.MapGet("/", async (string? siteKey, string? status, IQueryDispatcher dispatcher, CancellationToken ct) =>
            EndpointResults.Ok(await dispatcher.DispatchAsync(new GetMarketingCampaignsQuery(siteKey, status), ct)))
            .RequireAnyScope(ContentScopeNames.CampaignsView, ContentScopeNames.View);
        group.MapGet("/{id:guid}", async (Guid id, IQueryDispatcher dispatcher, HttpContext context, CancellationToken ct) =>
            EndpointResults.FromResult(await dispatcher.DispatchAsync(new GetMarketingCampaignByIdQuery(id), ct), context))
            .RequireAnyScope(ContentScopeNames.CampaignsView, ContentScopeNames.View);
        group.MapPost("/", async (CreateMarketingCampaignRequest request, ICommandDispatcher dispatcher, HttpContext context, CancellationToken ct) =>
            EndpointResults.FromResult(await dispatcher.DispatchAsync(new CreateMarketingCampaignCommand(request.SiteKey, request.Name, request.Slug,
                request.Status, request.StartsAtUtc, request.EndsAtUtc, request.LandingContentId, request.UtmSource, request.UtmMedium,
                request.UtmCampaign, request.GoalType, request.SettingsJson, context.GetCurrentUserId()), ct), context))
            .RequireAnyScope(ContentScopeNames.CampaignsEdit, ContentScopeNames.Edit);
        group.MapPut("/{id:guid}", async (Guid id, UpdateMarketingCampaignRequest request, ICommandDispatcher dispatcher, HttpContext context, CancellationToken ct) =>
            EndpointResults.FromResult(await dispatcher.DispatchAsync(new UpdateMarketingCampaignCommand(id, request.SiteKey, request.Name, request.Slug,
                request.Status, request.StartsAtUtc, request.EndsAtUtc, request.LandingContentId, request.UtmSource, request.UtmMedium,
                request.UtmCampaign, request.GoalType, request.SettingsJson, context.GetCurrentUserId()), ct), context))
            .RequireAnyScope(ContentScopeNames.CampaignsEdit, ContentScopeNames.Edit);
        group.MapDelete("/{id:guid}", async (Guid id, ICommandDispatcher dispatcher, HttpContext context, CancellationToken ct) =>
            EndpointResults.FromResult(await dispatcher.DispatchAsync(new DeleteMarketingCampaignCommand(id, context.GetCurrentUserId()), ct), context))
            .RequireAnyScope(ContentScopeNames.CampaignsEdit, ContentScopeNames.Edit);
        return app;
    }
}
