using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Queries;
using Dhole.Content.Application.Abstractions.Repositories;
using Dhole.Content.Contracts.Seo;
using Dhole.Content.Domain.Seo;
using Dhole.Content.Domain.Shared;

namespace Dhole.Content.Application.Seo;

public sealed record GetSeoPreviewQuery(Guid ContentId) : IQuery<Result<SeoPreviewDto>>;
public sealed record GetSitemapXmlQuery(string SiteKey, DateTime UtcNow) : IQuery<Result<string>>;
public sealed record GetRobotsTxtQuery(string SiteKey) : IQuery<Result<string>>;

public sealed class GetSeoPreviewQueryHandler(
    IContentItemRepository contents,
    ISiteRepository sites,
    IContentRouteRepository routes) : IQueryHandler<GetSeoPreviewQuery, Result<SeoPreviewDto>>
{
    public async Task<Result<SeoPreviewDto>> HandleAsync(GetSeoPreviewQuery query, CancellationToken cancellationToken = default)
    {
        var content = await contents.GetByIdAsync(query.ContentId, cancellationToken);
        if (content is null || content.IsDeleted) return Result.Failure<SeoPreviewDto>(ContentErrors.ContentNotFound);
        var site = await sites.GetBySiteKeyAsync(content.SiteKey, cancellationToken);
        if (site is null || site.IsDeleted) return Result.Failure<SeoPreviewDto>(ContentErrors.SiteNotFound);

        var route = await routes.GetPrimaryAsync(content.Id, content.Locale, cancellationToken);
        var baseUrl = $"https://{site.PrimaryDomain}";
        var fallbackPath = route?.Path ?? $"/{content.Slug}";
        var url = content.CanonicalUrl ?? $"{baseUrl.TrimEnd('/')}{fallbackPath}";
        var title = content.SeoTitle ?? content.Title;
        var description = content.SeoDescription ?? content.Excerpt;
        var imageMediaId = content.OpenGraphMediaId ?? content.FeaturedMediaId ?? site.DefaultOpenGraphMediaId;
        var robots = SeoRules.NormalizeRobots(content.Robots);

        var google = new SeoPlatformPreviewDto(title, description, url, null, "search-result");
        var facebook = new SeoPlatformPreviewDto(title, description, url, imageMediaId, "open-graph");
        var linkedIn = new SeoPlatformPreviewDto(title, description, url, imageMediaId, "open-graph");
        var twitter = new SeoPlatformPreviewDto(title, description, url, imageMediaId,
            imageMediaId.HasValue ? "summary_large_image" : "summary");

        return Result.Success(new SeoPreviewDto(content.Id, robots, content.SeoKeywords,
            content.StructuredDataJson, google, facebook, linkedIn, twitter));
    }
}

public sealed class GetSitemapXmlQueryHandler(
    ISiteRepository sites,
    ISeoRepository seoRepository) : IQueryHandler<GetSitemapXmlQuery, Result<string>>
{
    public async Task<Result<string>> HandleAsync(GetSitemapXmlQuery query, CancellationToken cancellationToken = default)
    {
        var site = await sites.GetBySiteKeyAsync(query.SiteKey, cancellationToken);
        if (site is null || site.IsDeleted) return Result.Failure<string>(ContentErrors.SiteNotFound);
        var baseUrl = $"https://{site.PrimaryDomain}".TrimEnd('/');
        var sources = await seoRepository.GetPublishedSitemapSourcesAsync(site.SiteKey, query.UtcNow, cancellationToken);
        var entries = sources.Select(source => new SitemapDocumentEntry(
            source.CanonicalUrl ?? $"{baseUrl}{source.Path}",
            source.LastModifiedUtc,
            source.Priority,
            source.ChangeFrequency));
        return Result.Success(SeoDocumentBuilder.BuildSitemap(entries));
    }
}

public sealed class GetRobotsTxtQueryHandler(ISiteRepository sites) : IQueryHandler<GetRobotsTxtQuery, Result<string>>
{
    public async Task<Result<string>> HandleAsync(GetRobotsTxtQuery query, CancellationToken cancellationToken = default)
    {
        var site = await sites.GetBySiteKeyAsync(query.SiteKey, cancellationToken);
        if (site is null || site.IsDeleted) return Result.Failure<string>(ContentErrors.SiteNotFound);
        return Result.Success(SeoDocumentBuilder.BuildRobots($"https://{site.PrimaryDomain}"));
    }
}
