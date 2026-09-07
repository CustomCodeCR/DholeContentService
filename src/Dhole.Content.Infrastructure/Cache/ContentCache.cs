using CustomCodeFramework.Redis.Abstractions;
using Dhole.Content.Application.Abstractions;

namespace Dhole.Content.Infrastructure.Cache;

public sealed class ContentCache(ICacheService cache) : IContentCache
{
    public Task RemoveContentAsync(string siteKey, string slug, CancellationToken cancellationToken)
        => cache.RemoveAsync(ContentCacheKeys.Content(siteKey, slug), cancellationToken);

    public Task RemoveMenuAsync(string siteKey, string location, CancellationToken cancellationToken)
        => cache.RemoveAsync(ContentCacheKeys.Menu(siteKey, location), cancellationToken);

    public Task RemoveSettingsAsync(string siteKey, CancellationToken cancellationToken)
        => cache.RemoveAsync(ContentCacheKeys.Settings(siteKey), cancellationToken);
}
