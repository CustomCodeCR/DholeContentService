using Dhole.Content.Api.Authorization;
using Dhole.Content.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Dhole.Content.Api.Endpoints;

/// <summary>
/// F6.13: scoped minimal REST status snapshots for navigation and settings;
/// site keys, values, unpublished content and leads are never returned.
/// </summary>
public static class RealtimeAccessEndpoints
{
    public static IEndpointRouteBuilder MapRealtimeAccessEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/content/realtime-access")
            .WithTags("Realtime Access")
            .RequireAuthorization();

        // Minimal status snapshots avoid returning unpublished navigation items,
        // site keys, private settings values or marketing data. The UI can
        // reconcile full content via the existing authorized REST queries.
        group.MapGet("/menu/{id:guid}", async (Guid id, ServiceDbContext db,
            CancellationToken ct) =>
        {
            var snapshot = await db.NavigationMenus.AsNoTracking()
                .Where(x => x.Id == id && !x.IsDeleted)
                .Select(x => new { x.Id, x.IsActive })
                .FirstOrDefaultAsync(ct);
            return snapshot is null ? Results.NotFound() : Results.Ok(snapshot);
        }).RequireScope(ContentScopeNames.View);

        group.MapGet("/setting/{id:guid}", async (Guid id, ServiceDbContext db,
            CancellationToken ct) =>
        {
            var snapshot = await db.SiteSettings.AsNoTracking()
                .Where(x => x.Id == id && !x.IsDeleted)
                .Select(x => new { x.Id, x.IsPublic })
                .FirstOrDefaultAsync(ct);
            return snapshot is null ? Results.NotFound() : Results.Ok(snapshot);
        }).RequireScope(ContentScopeNames.View);
        return app;
    }
}
