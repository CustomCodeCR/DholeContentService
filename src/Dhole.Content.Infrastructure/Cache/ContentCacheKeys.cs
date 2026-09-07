namespace Dhole.Content.Infrastructure.Cache;

public static class ContentCacheKeys
{
    public static string Content(string siteKey, string slug) => $"cms:{siteKey}:content:{slug}";
    public static string Menu(string siteKey, string location) => $"cms:{siteKey}:menu:{location}";
    public static string Settings(string siteKey) => $"cms:{siteKey}:settings";
}
