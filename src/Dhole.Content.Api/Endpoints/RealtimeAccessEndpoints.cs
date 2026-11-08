using Dhole.Content.Api.Authorization;
using Dhole.Content.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Dhole.Content.Api.Endpoints;

/// <summary>
/// F6.13: scoped no-content REST authorization checks for navigation and
/// settings resources; site keys, values, unpublished content and leads are
/// never returned in the check response.
/// </summary>
public static class RealtimeAccessEndpoints
{
    public static IEndpointRouteBuilder MapRealtimeAccessEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/content/realtime-access")
            .WithTags("Realtime Access")
            .RequireAuthorization();

        group.MapGet("/menu/{id:guid}", async (Guid id, ServiceDbContext db,
            CancellationToken ct) =>
            await db.NavigationMenus.AsNoTracking()
                .AnyAsync(x => x.Id == id && !x.IsDeleted, ct)
                ? Results.Ok() : Results.NotFound())
            .RequireScope(ContentScopeNames.View);

        group.MapGet("/setting/{id:guid}", async (Guid id, ServiceDbContext db,
            CancellationToken ct) =>
            await db.SiteSettings.AsNoTracking()
                .AnyAsync(x => x.Id == id && !x.IsDeleted, ct)
                ? Results.Ok() : Results.NotFound())
            .RequireScope(ContentScopeNames.View);
        return app;
    }
}
