namespace Dhole.Content.Infrastructure.Cache;

public static class ContentCacheKeys
{
    public static string Generation(string siteKey) => $"cms:public:{siteKey}:generation";
    public static string PublishedContent(string siteKey, string generation, string slug, string locale) => $"cms:public:{siteKey}:v:{generation}:{locale}:content:{slug}";
    public static string PublishedRoute(string siteKey, string generation, string locale, string path) => $"cms:public:{siteKey}:v:{generation}:{locale}:route:{path}";
    public static string Menu(string siteKey, string generation, string location) => $"cms:public:{siteKey}:v:{generation}:menu:{location}";
    public static string Placements(string siteKey, string generation) => $"cms:public:{siteKey}:v:{generation}:placements";
    public static string PlacementItems(string siteKey, string generation, Guid placementId) => $"cms:public:{siteKey}:v:{generation}:placement:{placementId:N}:items";
    public static string Collections(string siteKey, string generation) => $"cms:public:{siteKey}:v:{generation}:collections";
    public static string CollectionItems(string siteKey, string generation, Guid collectionId) => $"cms:public:{siteKey}:v:{generation}:collection:{collectionId:N}:items";
    public static string Settings(string siteKey, string generation) => $"cms:public:{siteKey}:v:{generation}:settings";
}
