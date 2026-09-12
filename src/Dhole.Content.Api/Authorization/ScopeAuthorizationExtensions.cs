using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Dhole.Content.Api.Authorization;

internal static class ScopeAuthorizationExtensions
{
    private const string Prefix = "Scope:";

    public static TBuilder RequireScope<TBuilder>(this TBuilder builder, string scope)
        where TBuilder : IEndpointConventionBuilder
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(scope);
        builder.RequireAuthorization($"{Prefix}{scope.Trim()}");
        return builder;
    }

    public static TBuilder RequireAnyScope<TBuilder>(this TBuilder builder, params string[] scopes)
        where TBuilder : IEndpointConventionBuilder
    {
        var required = scopes
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (required.Length == 0)
            throw new ArgumentException("At least one scope is required.", nameof(scopes));

        builder.RequireAuthorization(policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.RequireAssertion(context => HasAnyScope(context.User, required));
        });

        return builder;
    }

    internal static bool HasAnyScope(ClaimsPrincipal user, IReadOnlyCollection<string> required)
    {
        var granted = user.Claims
            .Where(claim => claim.Type.Equals("scope", StringComparison.OrdinalIgnoreCase)
                || claim.Type.Equals("scopes", StringComparison.OrdinalIgnoreCase))
            .SelectMany(claim => claim.Value.Split([' ', ',', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return required.Any(granted.Contains);
    }
}
