using CustomCodeFramework.Cqrs.Dispatching;
using Dhole.Content.Api.Authorization;
using Dhole.Content.Api.Extensions;
using Dhole.Content.Application.Leads;
using Dhole.Content.Contracts.Leads;

namespace Dhole.Content.Api.Endpoints;

public static class MarketingLeadEndpoints
{
    public static IEndpointRouteBuilder MapMarketingLeadEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/content/leads").WithTags("Marketing Leads").RequireAuthorization();

        group.MapGet("/", async (string? siteKey, string? status, Guid? ownerUserId, string? source, IQueryDispatcher dispatcher, CancellationToken ct) =>
            EndpointResults.Ok(await dispatcher.DispatchAsync(new GetMarketingLeadsQuery(siteKey, status, ownerUserId, source), ct)))
            .RequireAnyScope(ContentScopeNames.LeadsView, ContentScopeNames.View);
        group.MapGet("/{id:guid}", async (Guid id, IQueryDispatcher dispatcher, HttpContext context, CancellationToken ct) =>
            EndpointResults.FromResult(await dispatcher.DispatchAsync(new GetMarketingLeadByIdQuery(id), ct), context))
            .RequireAnyScope(ContentScopeNames.LeadsView, ContentScopeNames.View);

        group.MapPost("/", async (CreateMarketingLeadRequest request, ICommandDispatcher dispatcher, HttpContext context, CancellationToken ct) =>
            EndpointResults.FromResult(await dispatcher.DispatchAsync(new CreateMarketingLeadCommand(request.SiteKey, request.FirstName, request.LastName,
                request.Email, request.Phone, request.Company, request.JobTitle, request.Country, request.Source, request.Status, request.OwnerUserId,
                request.FirstTouchAtUtc, request.LastTouchAtUtc, context.GetCurrentUserId()), ct), context))
            .RequireAnyScope(ContentScopeNames.LeadsEdit, ContentScopeNames.Edit);
        group.MapPut("/{id:guid}", async (Guid id, UpdateMarketingLeadRequest request, ICommandDispatcher dispatcher, HttpContext context, CancellationToken ct) =>
            EndpointResults.FromResult(await dispatcher.DispatchAsync(new UpdateMarketingLeadCommand(id, request.SiteKey, request.FirstName, request.LastName,
                request.Email, request.Phone, request.Company, request.JobTitle, request.Country, request.Source, request.Status, request.OwnerUserId,
                request.LastTouchAtUtc, context.GetCurrentUserId()), ct), context))
            .RequireAnyScope(ContentScopeNames.LeadsEdit, ContentScopeNames.Edit);
        group.MapPost("/{id:guid}/touch", async (Guid id, TouchMarketingLeadRequest request, ICommandDispatcher dispatcher, HttpContext context, CancellationToken ct) =>
            EndpointResults.FromResult(await dispatcher.DispatchAsync(new TouchMarketingLeadCommand(id, request.Source, context.GetCurrentUserId()), ct), context))
            .RequireAnyScope(ContentScopeNames.LeadsEdit, ContentScopeNames.Edit);
        group.MapDelete("/{id:guid}", async (Guid id, ICommandDispatcher dispatcher, HttpContext context, CancellationToken ct) =>
            EndpointResults.FromResult(await dispatcher.DispatchAsync(new DeleteMarketingLeadCommand(id, context.GetCurrentUserId()), ct), context))
            .RequireAnyScope(ContentScopeNames.LeadsEdit, ContentScopeNames.Edit);
        return app;
    }
}
