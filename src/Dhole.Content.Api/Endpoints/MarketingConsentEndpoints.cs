using CustomCodeFramework.Cqrs.Dispatching;
using Dhole.Content.Api.Authorization;
using Dhole.Content.Api.Extensions;
using Dhole.Content.Application.Consents;
using Dhole.Content.Contracts.Consents;

namespace Dhole.Content.Api.Endpoints;

public static class MarketingConsentEndpoints
{
    public static IEndpointRouteBuilder MapMarketingConsentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/content/consents")
            .WithTags("Marketing Consents")
            .RequireAuthorization();

        group.MapGet("/", async (
                Guid? leadId,
                Guid? submissionId,
                string? purpose,
                bool? granted,
                DateTime? capturedFromUtc,
                DateTime? capturedToUtc,
                IQueryDispatcher dispatcher,
                CancellationToken ct) =>
            EndpointResults.Ok(await dispatcher.DispatchAsync(new GetMarketingConsentsQuery(
                leadId, submissionId, purpose, granted, capturedFromUtc, capturedToUtc), ct)))
            .RequireScope(ContentScopeNames.View);

        group.MapGet("/purposes", async (IQueryDispatcher dispatcher, CancellationToken ct) =>
            EndpointResults.Ok(await dispatcher.DispatchAsync(new GetMarketingConsentPurposesQuery(), ct)))
            .RequireScope(ContentScopeNames.View);

        group.MapGet("/{id:guid}", async (
                Guid id,
                IQueryDispatcher dispatcher,
                HttpContext context,
                CancellationToken ct) =>
            EndpointResults.FromResult(await dispatcher.DispatchAsync(
                new GetMarketingConsentByIdQuery(id), ct), context))
            .RequireScope(ContentScopeNames.View);

        group.MapPost("/", async (
                CaptureMarketingConsentRequest request,
                ICommandDispatcher dispatcher,
                HttpContext context,
                CancellationToken ct) =>
            EndpointResults.FromResult(await dispatcher.DispatchAsync(new CaptureMarketingConsentCommand(
                request.LeadId,
                request.SubmissionId,
                request.Purpose,
                request.Granted,
                request.PolicyVersion,
                request.Source,
                request.CapturedAtUtc,
                context.GetCurrentUserId()), ct), context))
            .RequireScope(ContentScopeNames.Edit);

        return app;
    }
}
