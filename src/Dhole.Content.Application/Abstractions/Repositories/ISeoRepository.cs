namespace Dhole.Content.Application.Abstractions.Repositories;

public sealed record SeoSitemapSource(
    string Path,
    string? CanonicalUrl,
    string? Robots,
    DateTime LastModifiedUtc,
    decimal? Priority,
    string? ChangeFrequency);

public interface ISeoRepository
{
    Task<IReadOnlyCollection<SeoSitemapSource>> GetPublishedSitemapSourcesAsync(
        string siteKey,
        DateTime utcNow,
        CancellationToken cancellationToken = default);
}
