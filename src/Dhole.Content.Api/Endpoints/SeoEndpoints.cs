using CustomCodeFramework.Cqrs.Dispatching;
using Dhole.Content.Api.Authorization;
using Dhole.Content.Api.Extensions;
using Dhole.Content.Application.Seo;
using Dhole.Content.Contracts.Seo;

namespace Dhole.Content.Api.Endpoints;

public static class SeoEndpoints
{
    public static IEndpointRouteBuilder MapSeoEndpoints(this IEndpointRouteBuilder app)
    {
        var admin = app.MapGroup("/api/content/seo").WithTags("Content SEO").RequireAuthorization();

        admin.MapGet("/{contentId:guid}/preview", async (Guid contentId, IQueryDispatcher dispatcher, HttpContext context, CancellationToken ct) =>
            EndpointResults.FromResult(await dispatcher.DispatchAsync(new GetSeoPreviewQuery(contentId), ct), context))
            .RequireScope(ContentScopeNames.View);

        admin.MapPut("/{contentId:guid}", async (Guid contentId, UpdateSeoRequest request, ICommandDispatcher dispatcher, HttpContext context, CancellationToken ct) =>
            EndpointResults.FromResult(await dispatcher.DispatchAsync(new UpdateSeoCommand(contentId, request.Title,
                request.Description, request.Keywords, request.CanonicalUrl, request.Robots, request.OpenGraphMediaId,
                request.StructuredDataJson, context.GetCurrentUserId()), ct), context))
            .RequireScope(ContentScopeNames.SeoEdit);

        var publicGroup = app.MapGroup("/api/content/public").WithTags("Public SEO").AllowAnonymous();
        publicGroup.MapGet("/sitemap.xml", SitemapAsync);
        publicGroup.MapGet("/robots.txt", RobotsAsync);
        app.MapGet("/sitemap.xml", SitemapAsync).AllowAnonymous();
        app.MapGet("/robots.txt", RobotsAsync).AllowAnonymous();

        return app;
    }

    private static async Task<IResult> SitemapAsync(string? siteKey, IQueryDispatcher dispatcher, CancellationToken ct)
    {
        var result = await dispatcher.DispatchAsync(new GetSitemapXmlQuery(string.IsNullOrWhiteSpace(siteKey) ? "main" : siteKey, DateTime.UtcNow), ct);
        return result.IsSuccess ? Results.Text(result.Value, "application/xml") : Results.NotFound();
    }

    private static async Task<IResult> RobotsAsync(string? siteKey, IQueryDispatcher dispatcher, CancellationToken ct)
    {
        var result = await dispatcher.DispatchAsync(new GetRobotsTxtQuery(string.IsNullOrWhiteSpace(siteKey) ? "main" : siteKey), ct);
        return result.IsSuccess ? Results.Text(result.Value, "text/plain") : Results.NotFound();
    }
}
