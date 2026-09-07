using System.Security.Claims;
using Dhole.Content.Api.Authorization;
using Dhole.Content.Application.Services;
using Dhole.Content.Contracts;

namespace Dhole.Content.Api.Endpoints;

public static class ContentEndpoints
{
    public static IEndpointRouteBuilder MapContentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/content").WithTags("CMS Content");

        group.MapGet("/", async (
            string? search,
            string? type,
            string? status,
            string? siteKey,
            int? page,
            int? pageSize,
            ContentApplicationService service,
            CancellationToken ct)
            => Results.Ok(await service.BrowseAsync(search, type, status, siteKey, page ?? 1, pageSize ?? 25, ct)))
            .RequireScope(ContentScopes.View);

        group.MapGet("/{id:guid}", async (Guid id, ContentApplicationService service, CancellationToken ct) =>
        {
            var result = await service.GetAsync(id, ct);
            return result is null ? Results.NotFound() : Results.Ok(result);
        }).RequireScope(ContentScopes.View);

        group.MapGet("/{id:guid}/preview", async (Guid id, ContentApplicationService service, CancellationToken ct) =>
        {
            var result = await service.GetAsync(id, ct);
            return result is null ? Results.NotFound() : Results.Ok(result);
        }).RequireScope(ContentScopes.View);

        group.MapPost("/", async (ContentWriteRequest request, HttpContext context, ContentApplicationService service, CancellationToken ct) =>
        {
            try
            {
                var result = await service.CreateAsync(request, UserId(context.User), ct);
                return Results.Created($"/api/v1/content/{result.Id}", result);
            }
            catch (Exception ex) { return Problem(ex); }
        }).RequireScope(ContentScopes.Create);

        group.MapPut("/{id:guid}", async (Guid id, ContentWriteRequest request, HttpContext context, ContentApplicationService service, CancellationToken ct) =>
        {
            try { return Results.Ok(await service.UpdateAsync(id, request, UserId(context.User), ct)); }
            catch (Exception ex) { return Problem(ex); }
        }).RequireScope(ContentScopes.Edit);

        group.MapPost("/{id:guid}/submit-review", async (Guid id, HttpContext context, ContentApplicationService service, CancellationToken ct) =>
        {
            try { await service.SubmitForReviewAsync(id, UserId(context.User), ct); return Results.NoContent(); }
            catch (Exception ex) { return Problem(ex); }
        }).RequireScope(ContentScopes.Edit);

        group.MapPost("/{id:guid}/publish", async (Guid id, HttpContext context, ContentApplicationService service, CancellationToken ct) =>
        {
            try { await service.PublishAsync(id, UserId(context.User), ct); return Results.NoContent(); }
            catch (Exception ex) { return Problem(ex); }
        }).RequireScope(ContentScopes.Publish);

        group.MapPost("/{id:guid}/schedule", async (Guid id, ScheduleContentRequest request, HttpContext context, ContentApplicationService service, CancellationToken ct) =>
        {
            try { await service.ScheduleAsync(id, request.ScheduledAtUtc, UserId(context.User), ct); return Results.NoContent(); }
            catch (Exception ex) { return Problem(ex); }
        }).RequireScope(ContentScopes.Publish);

        group.MapPost("/{id:guid}/unpublish", async (Guid id, HttpContext context, ContentApplicationService service, CancellationToken ct) =>
        {
            try { await service.UnpublishAsync(id, UserId(context.User), ct); return Results.NoContent(); }
            catch (Exception ex) { return Problem(ex); }
        }).RequireScope(ContentScopes.Publish);

        group.MapPost("/{id:guid}/archive", async (Guid id, HttpContext context, ContentApplicationService service, CancellationToken ct) =>
        {
            try { await service.ArchiveAsync(id, UserId(context.User), ct); return Results.NoContent(); }
            catch (Exception ex) { return Problem(ex); }
        }).RequireScope(ContentScopes.Edit);

        group.MapDelete("/{id:guid}", async (Guid id, HttpContext context, ContentApplicationService service, CancellationToken ct) =>
        {
            try { await service.DeleteAsync(id, UserId(context.User), ct); return Results.NoContent(); }
            catch (Exception ex) { return Problem(ex); }
        }).RequireScope(ContentScopes.Delete);

        group.MapGet("/{id:guid}/revisions", async (Guid id, ContentApplicationService service, CancellationToken ct)
            => Results.Ok(await service.RevisionsAsync(id, ct)))
            .RequireScope(ContentScopes.View);

        group.MapPost("/{id:guid}/revisions/{revisionId:guid}/restore", async (
            Guid id,
            Guid revisionId,
            RevisionRestoreRequest _,
            HttpContext context,
            ContentApplicationService service,
            CancellationToken ct) =>
        {
            try { await service.RestoreRevisionAsync(id, revisionId, UserId(context.User), ct); return Results.NoContent(); }
            catch (Exception ex) { return Problem(ex); }
        }).RequireScope(ContentScopes.Edit);

        return app;
    }

    private static Guid? UserId(ClaimsPrincipal user)
    {
        var raw = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub") ?? user.FindFirstValue("user_id");
        return Guid.TryParse(raw, out var id) ? id : null;
    }

    private static IResult Problem(Exception ex)
        => ex switch
        {
            KeyNotFoundException => Results.Problem(ex.Message, statusCode: StatusCodes.Status404NotFound),
            ArgumentException or InvalidOperationException => Results.Problem(ex.Message, statusCode: StatusCodes.Status400BadRequest),
            _ => Results.Problem("Error procesando la operación de contenido.", statusCode: StatusCodes.Status500InternalServerError)
        };
}
