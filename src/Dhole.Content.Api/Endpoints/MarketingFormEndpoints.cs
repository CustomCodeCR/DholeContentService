using CustomCodeFramework.Cqrs.Dispatching;
using Dhole.Content.Api.Authorization;
using Dhole.Content.Api.Extensions;
using Dhole.Content.Application.Forms;
using Dhole.Content.Contracts.Forms;

namespace Dhole.Content.Api.Endpoints;

public static class MarketingFormEndpoints
{
    public static IEndpointRouteBuilder MapMarketingFormEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/content/forms")
            .WithTags("Marketing Forms")
            .RequireAuthorization();

        group.MapGet("/", async (
                string? siteKey,
                string? purpose,
                string? status,
                IQueryDispatcher dispatcher,
                CancellationToken ct) =>
            EndpointResults.Ok(await dispatcher.DispatchAsync(new GetMarketingFormsQuery(siteKey, purpose, status), ct)))
            .RequireScope(ContentScopeNames.View);

        group.MapGet("/purposes", async (IQueryDispatcher dispatcher, CancellationToken ct) =>
            EndpointResults.Ok(await dispatcher.DispatchAsync(new GetMarketingFormPurposesQuery(), ct)))
            .RequireScope(ContentScopeNames.View);

        group.MapGet("/{id:guid}", async (
                Guid id,
                IQueryDispatcher dispatcher,
                HttpContext context,
                CancellationToken ct) =>
            EndpointResults.FromResult(await dispatcher.DispatchAsync(new GetMarketingFormByIdQuery(id), ct), context))
            .RequireScope(ContentScopeNames.View);

        group.MapPost("/", async (
                CreateMarketingFormRequest request,
                ICommandDispatcher dispatcher,
                HttpContext context,
                CancellationToken ct) =>
            EndpointResults.FromResult(await dispatcher.DispatchAsync(new CreateMarketingFormCommand(
                request.SiteKey,
                request.FormKey,
                request.Name,
                request.Purpose,
                request.Status,
                request.SuccessMessage,
                request.NotificationTemplateKey,
                request.SettingsJson,
                context.GetCurrentUserId()), ct), context))
            .RequireScope(ContentScopeNames.Edit);

        group.MapPut("/{id:guid}", async (
                Guid id,
                UpdateMarketingFormRequest request,
                ICommandDispatcher dispatcher,
                HttpContext context,
                CancellationToken ct) =>
            EndpointResults.FromResult(await dispatcher.DispatchAsync(new UpdateMarketingFormCommand(
                id,
                request.SiteKey,
                request.FormKey,
                request.Name,
                request.Purpose,
                request.Status,
                request.SuccessMessage,
                request.NotificationTemplateKey,
                request.SettingsJson,
                context.GetCurrentUserId()), ct), context))
            .RequireScope(ContentScopeNames.Edit);

        group.MapDelete("/{id:guid}", async (
                Guid id,
                ICommandDispatcher dispatcher,
                HttpContext context,
                CancellationToken ct) =>
            EndpointResults.FromResult(await dispatcher.DispatchAsync(
                new DeleteMarketingFormCommand(id, context.GetCurrentUserId()), ct), context))
            .RequireScope(ContentScopeNames.Edit);

        group.MapGet("/{formId:guid}/fields", async (
                Guid formId,
                IQueryDispatcher dispatcher,
                HttpContext context,
                CancellationToken ct) =>
            EndpointResults.FromResult(await dispatcher.DispatchAsync(new GetMarketingFormFieldsQuery(formId), ct), context))
            .RequireScope(ContentScopeNames.View);

        group.MapPost("/{formId:guid}/fields", async (
                Guid formId,
                CreateMarketingFormFieldRequest request,
                ICommandDispatcher dispatcher,
                HttpContext context,
                CancellationToken ct) =>
            EndpointResults.FromResult(await dispatcher.DispatchAsync(new CreateMarketingFormFieldCommand(
                formId,
                request.FieldKey,
                request.Label,
                request.FieldType,
                request.Placeholder,
                request.IsRequired,
                request.SortOrder,
                request.ValidationJson,
                request.OptionsJson,
                context.GetCurrentUserId()), ct), context))
            .RequireScope(ContentScopeNames.Edit);

        group.MapPut("/{formId:guid}/fields/{fieldId:guid}", async (
                Guid formId,
                Guid fieldId,
                UpdateMarketingFormFieldRequest request,
                ICommandDispatcher dispatcher,
                HttpContext context,
                CancellationToken ct) =>
            EndpointResults.FromResult(await dispatcher.DispatchAsync(new UpdateMarketingFormFieldCommand(
                formId,
                fieldId,
                request.FieldKey,
                request.Label,
                request.FieldType,
                request.Placeholder,
                request.IsRequired,
                request.SortOrder,
                request.ValidationJson,
                request.OptionsJson,
                context.GetCurrentUserId()), ct), context))
            .RequireScope(ContentScopeNames.Edit);

        group.MapDelete("/{formId:guid}/fields/{fieldId:guid}", async (
                Guid formId,
                Guid fieldId,
                ICommandDispatcher dispatcher,
                HttpContext context,
                CancellationToken ct) =>
            EndpointResults.FromResult(await dispatcher.DispatchAsync(
                new DeleteMarketingFormFieldCommand(formId, fieldId, context.GetCurrentUserId()), ct), context))
            .RequireScope(ContentScopeNames.Edit);

        return app;
    }
}
