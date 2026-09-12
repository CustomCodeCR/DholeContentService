using CustomCodeFramework.Cqrs.Dispatching;
using Dhole.Content.Api.Authorization;
using Dhole.Content.Api.Extensions;
using Dhole.Content.Application.Meetings;
using Dhole.Content.Contracts.Meetings;

namespace Dhole.Content.Api.Endpoints;

public static class MeetingEndpoints
{
    public static IEndpointRouteBuilder MapMeetingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/content/meetings").WithTags("Meetings").RequireAuthorization();

        group.MapGet("/types", async (string? siteKey, bool? isActive, IQueryDispatcher dispatcher, CancellationToken ct) =>
            EndpointResults.Ok(await dispatcher.DispatchAsync(new GetMeetingTypesQuery(siteKey, isActive), ct)))
            .RequireScope(ContentScopeNames.View);
        group.MapGet("/types/{id:guid}", async (Guid id, IQueryDispatcher dispatcher, HttpContext context, CancellationToken ct) =>
            EndpointResults.FromResult(await dispatcher.DispatchAsync(new GetMeetingTypeByIdQuery(id), ct), context))
            .RequireScope(ContentScopeNames.View);
        group.MapPost("/types", async (CreateMeetingTypeRequest request, ICommandDispatcher dispatcher, HttpContext context, CancellationToken ct) =>
            EndpointResults.FromResult(await dispatcher.DispatchAsync(new CreateMeetingTypeCommand(request.SiteKey, request.Name, request.Slug,
                request.Description, request.DurationMinutes, request.BufferMinutes, request.MeetingMode, request.AssignedUserId,
                request.AssignedTeamKey, request.SettingsJson, request.IsActive, context.GetCurrentUserId()), ct), context))
            .RequireScope(ContentScopeNames.Edit);
        group.MapPut("/types/{id:guid}", async (Guid id, UpdateMeetingTypeRequest request, ICommandDispatcher dispatcher, HttpContext context, CancellationToken ct) =>
            EndpointResults.FromResult(await dispatcher.DispatchAsync(new UpdateMeetingTypeCommand(id, request.SiteKey, request.Name, request.Slug,
                request.Description, request.DurationMinutes, request.BufferMinutes, request.MeetingMode, request.AssignedUserId,
                request.AssignedTeamKey, request.SettingsJson, request.IsActive, context.GetCurrentUserId()), ct), context))
            .RequireScope(ContentScopeNames.Edit);
        group.MapDelete("/types/{id:guid}", async (Guid id, ICommandDispatcher dispatcher, HttpContext context, CancellationToken ct) =>
            EndpointResults.FromResult(await dispatcher.DispatchAsync(new DeleteMeetingTypeCommand(id, context.GetCurrentUserId()), ct), context))
            .RequireScope(ContentScopeNames.Edit);

        group.MapGet("/requests", async (Guid? meetingTypeId, string? status, Guid? assignedUserId, DateTime? fromUtc, DateTime? toUtc,
                IQueryDispatcher dispatcher, CancellationToken ct) =>
            EndpointResults.Ok(await dispatcher.DispatchAsync(new GetMeetingRequestsQuery(meetingTypeId, status, assignedUserId, fromUtc, toUtc), ct)))
            .RequireScope(ContentScopeNames.View);
        group.MapGet("/requests/{id:guid}", async (Guid id, IQueryDispatcher dispatcher, HttpContext context, CancellationToken ct) =>
            EndpointResults.FromResult(await dispatcher.DispatchAsync(new GetMeetingRequestByIdQuery(id), ct), context))
            .RequireScope(ContentScopeNames.View);
        group.MapPost("/requests", async (CreateMeetingRequest request, ICommandDispatcher dispatcher, HttpContext context, CancellationToken ct) =>
            EndpointResults.FromResult(await dispatcher.DispatchAsync(new CreateMeetingRequestCommand(request.MeetingTypeId, request.LeadId,
                request.SubmissionId, request.RequestedStartUtc, request.RequestedEndUtc, request.TimeZone, request.Subject,
                request.Message, context.GetCurrentUserId()), ct), context))
            .RequireScope(ContentScopeNames.Edit);
        group.MapPost("/requests/{id:guid}/pending-confirmation", async (Guid id, PendingMeetingRequest request, ICommandDispatcher dispatcher, HttpContext context, CancellationToken ct) =>
            EndpointResults.FromResult(await dispatcher.DispatchAsync(new MarkMeetingPendingCommand(id, request.AssignedUserId, context.GetCurrentUserId()), ct), context))
            .RequireScope(ContentScopeNames.Edit);
        group.MapPost("/requests/{id:guid}/confirm", async (Guid id, ConfirmMeetingRequest request, ICommandDispatcher dispatcher, HttpContext context, CancellationToken ct) =>
            EndpointResults.FromResult(await dispatcher.DispatchAsync(new ConfirmMeetingCommand(id, request.ConfirmedStartUtc, request.ConfirmedEndUtc,
                request.AssignedUserId, request.ExternalProvider, request.ExternalEventId, request.MeetingUrl, context.GetCurrentUserId()), ct), context))
            .RequireScope(ContentScopeNames.Edit);
        group.MapPost("/requests/{id:guid}/reject", async (Guid id, ICommandDispatcher dispatcher, HttpContext context, CancellationToken ct) =>
            EndpointResults.FromResult(await dispatcher.DispatchAsync(new RejectMeetingCommand(id, context.GetCurrentUserId()), ct), context))
            .RequireScope(ContentScopeNames.Edit);
        group.MapPost("/requests/{id:guid}/cancel", async (Guid id, ICommandDispatcher dispatcher, HttpContext context, CancellationToken ct) =>
            EndpointResults.FromResult(await dispatcher.DispatchAsync(new CancelMeetingCommand(id, context.GetCurrentUserId()), ct), context))
            .RequireScope(ContentScopeNames.Edit);
        group.MapPost("/requests/{id:guid}/complete", async (Guid id, ICommandDispatcher dispatcher, HttpContext context, CancellationToken ct) =>
            EndpointResults.FromResult(await dispatcher.DispatchAsync(new CompleteMeetingCommand(id, context.GetCurrentUserId()), ct), context))
            .RequireScope(ContentScopeNames.Edit);

        return app;
    }
}
