using Dhole.Content.Domain.Content;

namespace Dhole.Content.Application.Abstractions;

public interface IContentRepository
{
    Task<ContentItem?> GetAsync(Guid id, bool includeRevisions, CancellationToken cancellationToken);
    Task<ContentItem?> GetPublishedBySlugAsync(string slug, string siteKey, string? locale, CancellationToken cancellationToken);
    Task<(IReadOnlyCollection<ContentItem> Items, long Total)> BrowseAsync(string? search, ContentType? type, ContentStatus? status, string siteKey, int page, int pageSize, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<ContentItem>> GetScheduledDueAsync(DateTime utcNow, CancellationToken cancellationToken);
    Task<bool> SlugExistsAsync(string slug, string siteKey, Guid? exceptId, CancellationToken cancellationToken);
    Task AddAsync(ContentItem item, CancellationToken cancellationToken);
    void AddRevision(ContentRevision revision);
    Task<ContentRevision?> GetRevisionAsync(Guid contentId, Guid revisionId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<ContentRevision>> GetRevisionsAsync(Guid contentId, CancellationToken cancellationToken);
    Task ReplaceTaxonomiesAsync(Guid contentId, IReadOnlyCollection<Guid> taxonomyTermIds, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<TaxonomyTerm>> GetTaxonomiesAsync(string siteKey, string? kind, CancellationToken cancellationToken);
    Task AddTaxonomyAsync(TaxonomyTerm term, CancellationToken cancellationToken);

    Task AddMediaAsync(MediaReference media, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<MediaReference>> GetMediaAsync(string? search, int page, int pageSize, CancellationToken cancellationToken);

    Task<NavigationMenu?> GetMenuAsync(string location, string siteKey, CancellationToken cancellationToken);
    Task ReplaceMenuAsync(NavigationMenu menu, IReadOnlyCollection<NavigationMenuItem> items, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<SiteSetting>> GetSettingsAsync(string siteKey, bool publicOnly, CancellationToken cancellationToken);
    Task<SiteSetting?> GetSettingAsync(string key, string siteKey, CancellationToken cancellationToken);
    Task AddSettingAsync(SiteSetting setting, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}

public interface IContentCache
{
    Task RemoveContentAsync(string siteKey, string slug, CancellationToken cancellationToken);
    Task RemoveMenuAsync(string siteKey, string location, CancellationToken cancellationToken);
    Task RemoveSettingsAsync(string siteKey, CancellationToken cancellationToken);
}

public interface IContentEventPublisher
{
    Task QueueAsync(ContentItem item, string eventName, Guid? actorUserId, CancellationToken cancellationToken);
}
