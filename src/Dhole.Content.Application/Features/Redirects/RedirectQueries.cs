using CustomCodeFramework.Cqrs.Queries;
using Dhole.Content.Application.Abstractions.Repositories;
using Dhole.Content.Contracts.Redirects;
using Dhole.Content.Domain.Redirects.Entities;

namespace Dhole.Content.Application.Redirects;

public sealed record GetRedirectsQuery(string? SiteKey) : IQuery<IReadOnlyCollection<ContentRedirectDto>>;
public sealed record ResolveRedirectQuery(string SiteKey, string SourcePath, DateTime UtcNow) : IQuery<RedirectResolutionDto?>;

public sealed class GetRedirectsQueryHandler(IRedirectRepository redirects)
    : IQueryHandler<GetRedirectsQuery, IReadOnlyCollection<ContentRedirectDto>>
{
    public async Task<IReadOnlyCollection<ContentRedirectDto>> HandleAsync(GetRedirectsQuery query, CancellationToken cancellationToken = default)
        => (await redirects.GetAllAsync(query.SiteKey, cancellationToken)).Select(Map).ToArray();

    internal static ContentRedirectDto Map(ContentRedirect redirect)
        => new(redirect.Id, redirect.SiteKey, redirect.SourcePath, redirect.TargetUrl, redirect.StatusCode,
            redirect.IsActive, redirect.ValidFromUtc, redirect.ValidToUtc, redirect.CreatedAtUtc, redirect.UpdatedAtUtc);
}

public sealed class ResolveRedirectQueryHandler(IRedirectRepository redirects)
    : IQueryHandler<ResolveRedirectQuery, RedirectResolutionDto?>
{
    public async Task<RedirectResolutionDto?> HandleAsync(ResolveRedirectQuery query, CancellationToken cancellationToken = default)
    {
        var redirect = await redirects.ResolveAsync(query.SiteKey, query.SourcePath, query.UtcNow, cancellationToken);
        return redirect is null ? null : new RedirectResolutionDto(redirect.TargetUrl, redirect.StatusCode);
    }
}
