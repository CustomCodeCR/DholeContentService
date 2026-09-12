using CustomCodeFramework.Cqrs.Dispatching;
using Dhole.Content.Api.Authorization;
using Dhole.Content.Api.Extensions;
using Dhole.Content.Application.Navigation.GetNavigationMenu;
using Dhole.Content.Application.Navigation.SetNavigationMenuActive;
using Dhole.Content.Application.Navigation.UpsertNavigationMenu;
using Dhole.Content.Contracts.Navigation;

namespace Dhole.Content.Api.Endpoints;

public static class NavigationEndpoints
{
    public static IEndpointRouteBuilder MapNavigationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/content/menus").WithTags("Content Menus").RequireAuthorization();

        group.MapGet("/{location}", async (string location, string? siteKey, IQueryDispatcher dispatcher, HttpContext context, CancellationToken ct) =>
            EndpointResults.FromResult(await dispatcher.DispatchAsync(new GetNavigationMenuQuery(siteKey ?? "main", location), ct), context))
            .RequireScope(ContentScopeNames.View);

        group.MapPut("/{location}", async (string location, UpsertNavigationMenuRequest request, ICommandDispatcher dispatcher, HttpContext context, CancellationToken ct) =>
            EndpointResults.FromResult(await dispatcher.DispatchAsync(new UpsertNavigationMenuCommand(request.Name, location, request.SiteKey,
                request.Items.Select(item => new NavigationMenuItemInput(item.Label, item.Url, item.ContentId, item.ParentId, item.SortOrder, item.Target)).ToArray(),
                context.GetCurrentUserId()), ct), context))
            .RequireAnyScope(ContentScopeNames.NavigationEdit, ContentScopeNames.SettingsEdit);

        group.MapPatch("/{id:guid}/active", async (Guid id, bool isActive, ICommandDispatcher dispatcher, HttpContext context, CancellationToken ct) =>
            EndpointResults.FromResult(await dispatcher.DispatchAsync(new SetNavigationMenuActiveCommand(id, isActive, context.GetCurrentUserId()), ct), context))
            .RequireAnyScope(ContentScopeNames.NavigationEdit, ContentScopeNames.SettingsEdit);

        return app;
    }
}
