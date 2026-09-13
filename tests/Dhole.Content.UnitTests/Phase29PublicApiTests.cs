using Dhole.Content.Api.Endpoints;
using Dhole.Content.Domain.ContentItems.Entities;
using Dhole.Content.Domain.ContentItems.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Xunit;

namespace Dhole.Content.UnitTests;

public sealed class Phase29PublicApiTests
{
    [Fact]
    public void CmsAdminAliases_MirrorOnlyAuthenticatedContentEndpoints()
    {
        var builder = WebApplication.CreateBuilder();
        var app = builder.Build();

        app.MapGet("/api/content/items", () => Results.Ok()).RequireAuthorization("cms.items.view");
        app.MapPost("/api/content/forms/{id:guid}/submissions", (Guid id) => Results.Ok(id)).AllowAnonymous();
        app.MapGet("/api/content/public/pages", () => Results.Ok()).AllowAnonymous();

        app.MapCmsAdminAliases();

        var endpoints = app.DataSources.SelectMany(source => source.Endpoints).OfType<RouteEndpoint>().ToArray();
        var cmsItems = Assert.Single(endpoints.Where(endpoint => endpoint.RoutePattern.RawText == "/api/cms/items"));
        Assert.NotEmpty(cmsItems.Metadata.GetOrderedMetadata<IAuthorizeData>());
        Assert.Null(cmsItems.Metadata.GetMetadata<IAllowAnonymous>());
        Assert.DoesNotContain(endpoints, endpoint => endpoint.RoutePattern.RawText == "/api/cms/forms/{id:guid}/submissions");
        Assert.DoesNotContain(endpoints, endpoint => endpoint.RoutePattern.RawText == "/api/cms/public/pages");
    }

    [Fact]
    public void PublicContentPolicy_OnlyAcceptsPublishedNonDeletedContent()
    {
        var draft = ContentItem.Create(ContentType.Page, "Draft", "draft", "[]", null, null);
        Assert.False(PublicContentPolicy.IsPublished(draft));

        var published = ContentItem.Create(ContentType.Page, "Published", "published", "[]", null, null);
        published.SubmitForReview(null);
        published.Publish(DateTime.UtcNow, null);
        Assert.True(PublicContentPolicy.IsPublished(published));

        published.Delete(null);
        Assert.False(PublicContentPolicy.IsPublished(published));
    }

    [Theory]
    [InlineData(true, -1, 1, true)]
    [InlineData(false, -1, 1, false)]
    [InlineData(true, 1, 2, false)]
    [InlineData(true, -2, -1, false)]
    public void PublicContentPolicy_RespectsActivePlacementWindows(bool active, int startHours, int endHours, bool expected)
    {
        var now = DateTime.UtcNow;
        Assert.Equal(expected, PublicContentPolicy.IsCurrentlyActive(
            active,
            now.AddHours(startHours),
            now.AddHours(endHours),
            now));
    }
}
