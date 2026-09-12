using CustomCodeFramework.Cqrs.Dispatching;
using Dhole.Content.Api.Authorization;
using Dhole.Content.Api.Extensions;
using Dhole.Content.Application.Redirects;
using Dhole.Content.Contracts.Redirects;

namespace Dhole.Content.Api.Endpoints;

public static class RedirectEndpoints
{
    public static IEndpointRouteBuilder MapRedirectEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/content/redirects")
            .WithTags("Content Redirects")
            .RequireAuthorization();

        group.MapGet("/", async (string? siteKey, IQueryDispatcher dispatcher, CancellationToken ct) =>
            EndpointResults.Ok(await dispatcher.DispatchAsync(new GetRedirectsQuery(siteKey), ct)))
            .RequireScope(ContentScopeNames.View);

        group.MapPost("/", async (CreateRedirectRequest request, ICommandDispatcher dispatcher, HttpContext context, CancellationToken ct) =>
            EndpointResults.FromResult(await dispatcher.DispatchAsync(new CreateRedirectCommand(request.SiteKey, request.SourcePath,
                request.TargetUrl, request.StatusCode, request.IsActive, request.ValidFromUtc, request.ValidToUtc,
                context.GetCurrentUserId()), ct), context))
            .RequireScope(ContentScopeNames.Edit);

        group.MapPut("/{id:guid}", async (Guid id, UpdateRedirectRequest request, ICommandDispatcher dispatcher, HttpContext context, CancellationToken ct) =>
            EndpointResults.FromResult(await dispatcher.DispatchAsync(new UpdateRedirectCommand(id, request.SiteKey, request.SourcePath,
                request.TargetUrl, request.StatusCode, request.IsActive, request.ValidFromUtc, request.ValidToUtc,
                context.GetCurrentUserId()), ct), context))
            .RequireScope(ContentScopeNames.Edit);

        group.MapDelete("/{id:guid}", async (Guid id, ICommandDispatcher dispatcher, HttpContext context, CancellationToken ct) =>
            EndpointResults.FromResult(await dispatcher.DispatchAsync(new DeleteRedirectCommand(id, context.GetCurrentUserId()), ct), context))
            .RequireScope(ContentScopeNames.Edit);

        app.MapGet("/api/content/public/redirect", async (string? siteKey, string path, IQueryDispatcher dispatcher, CancellationToken ct) =>
            {
                var resolved = await dispatcher.DispatchAsync(new ResolveRedirectQuery(
                    string.IsNullOrWhiteSpace(siteKey) ? "main" : siteKey, path, DateTime.UtcNow), ct);
                return resolved is null ? Results.NotFound() : EndpointResults.Ok(resolved);
            })
            .WithTags("Public Content Redirects")
            .AllowAnonymous();

        return app;
    }
}
