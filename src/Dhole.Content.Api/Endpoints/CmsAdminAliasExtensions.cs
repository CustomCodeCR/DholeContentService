using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.Primitives;

namespace Dhole.Content.Api.Endpoints;

public static class CmsAdminAliasExtensions
{
    private const string LegacyPrefix = "/api/content";
    private const string CmsPrefix = "/api/cms";

    public static IEndpointRouteBuilder MapCmsAdminAliases(this IEndpointRouteBuilder app)
    {
        var aliases = app.DataSources
            .SelectMany(source => source.Endpoints)
            .OfType<RouteEndpoint>()
            .Where(IsAdministrativeContentEndpoint)
            .Select(CreateAlias)
            .Cast<Endpoint>()
            .ToArray();

        if (aliases.Length > 0)
        {
            app.DataSources.Add(new StaticEndpointDataSource(aliases));
        }

        return app;
    }

    private static bool IsAdministrativeContentEndpoint(RouteEndpoint endpoint)
    {
        var route = endpoint.RoutePattern.RawText;
        if (string.IsNullOrWhiteSpace(route) || !route.StartsWith(LegacyPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (route.StartsWith($"{LegacyPrefix}/public", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (endpoint.Metadata.GetMetadata<IAllowAnonymous>() is not null)
        {
            return false;
        }

        return endpoint.Metadata.GetOrderedMetadata<IAuthorizeData>().Count > 0;
    }

    private static RouteEndpoint CreateAlias(RouteEndpoint endpoint)
    {
        var legacyRoute = endpoint.RoutePattern.RawText!;
        var cmsRoute = $"{CmsPrefix}{legacyRoute[LegacyPrefix.Length..]}";
        var builder = new RouteEndpointBuilder(
            endpoint.RequestDelegate ?? throw new InvalidOperationException($"Endpoint {legacyRoute} does not have a request delegate."),
            RoutePatternFactory.Parse(cmsRoute),
            endpoint.Order)
        {
            DisplayName = $"CMS {endpoint.DisplayName ?? cmsRoute}"
        };

        foreach (var metadata in endpoint.Metadata)
        {
            builder.Metadata.Add(metadata);
        }

        return (RouteEndpoint)builder.Build();
    }

    private sealed class StaticEndpointDataSource(IReadOnlyList<Endpoint> endpoints) : EndpointDataSource
    {
        public override IReadOnlyList<Endpoint> Endpoints { get; } = endpoints;
        public override IChangeToken GetChangeToken() => NeverChangeToken.Instance;
    }

    private sealed class NeverChangeToken : IChangeToken
    {
        public static NeverChangeToken Instance { get; } = new();
        public bool HasChanged => false;
        public bool ActiveChangeCallbacks => false;
        public IDisposable RegisterChangeCallback(Action<object?> callback, object? state) => NoopDisposable.Instance;
    }

    private sealed class NoopDisposable : IDisposable
    {
        public static NoopDisposable Instance { get; } = new();
        public void Dispose() { }
    }
}
