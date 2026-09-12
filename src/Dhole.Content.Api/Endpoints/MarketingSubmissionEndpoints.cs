using System.Text.Json;
using CustomCodeFramework.Cqrs.Dispatching;
using Dhole.Content.Api.Authorization;
using Dhole.Content.Application.Submissions;
using Dhole.Content.Contracts.Submissions;
using Dhole.Content.Domain.Submissions;

namespace Dhole.Content.Api.Endpoints;

public static class MarketingSubmissionEndpoints
{
    public static IEndpointRouteBuilder MapMarketingSubmissionEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/content/forms/{formId:guid}/submissions", async (
                Guid formId,
                SubmitMarketingFormRequest request,
                ICommandDispatcher dispatcher,
                HttpContext context,
                CancellationToken ct) =>
            {
                var payloadJson = request.Payload.ValueKind == JsonValueKind.Undefined
                    ? string.Empty
                    : request.Payload.GetRawText();
                var correlationId = ResolveCorrelationId(context);
                var referrerUrl = string.IsNullOrWhiteSpace(request.ReferrerUrl)
                    ? context.Request.Headers["Referer"].ToString()
                    : request.ReferrerUrl;
                var ipHash = SubmissionRules.HashIp(context.Connection.RemoteIpAddress?.ToString());
                var userAgent = context.Request.Headers["User-Agent"].ToString();

                return EndpointResults.FromResult(await dispatcher.DispatchAsync(
                    new SubmitMarketingFormCommand(
                        formId,
                        request.ContentId,
                        request.CampaignId,
                        request.SourceUrl,
                        referrerUrl,
                        request.UtmSource,
                        request.UtmMedium,
                        request.UtmCampaign,
                        request.UtmContent,
                        request.UtmTerm,
                        payloadJson,
                        ipHash,
                        userAgent,
                        correlationId,
                        request.Consents), ct), context);
            })
            .WithTags("Marketing Submissions")
            .AllowAnonymous();

        var admin = app.MapGroup("/api/content/submissions")
            .WithTags("Marketing Submissions")
            .RequireAuthorization();

        admin.MapGet("/", async (
                Guid? formId,
                string? status,
                DateTime? submittedFromUtc,
                DateTime? submittedToUtc,
                IQueryDispatcher dispatcher,
                CancellationToken ct) =>
            EndpointResults.Ok(await dispatcher.DispatchAsync(
                new GetMarketingSubmissionsQuery(formId, status, submittedFromUtc, submittedToUtc), ct)))
            .RequireScope(ContentScopeNames.View);

        admin.MapGet("/{id:guid}", async (
                Guid id,
                IQueryDispatcher dispatcher,
                HttpContext context,
                CancellationToken ct) =>
            EndpointResults.FromResult(await dispatcher.DispatchAsync(
                new GetMarketingSubmissionByIdQuery(id), ct), context))
            .RequireScope(ContentScopeNames.View);

        return app;
    }

    private static Guid ResolveCorrelationId(HttpContext context)
    {
        var value = context.Request.Headers["X-Correlation-ID"].FirstOrDefault();
        return Guid.TryParse(value, out var correlationId) && correlationId != Guid.Empty
            ? correlationId
            : Guid.NewGuid();
    }
}
