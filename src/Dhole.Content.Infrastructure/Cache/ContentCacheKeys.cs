namespace Dhole.Content.Infrastructure.Cache;
internal static class ContentCacheKeys
{
    public static string PublishedContent(string siteKey,string slug,string locale)=>$"cms:public:{siteKey}:{locale}:content:{slug}";
    public static string Menu(string siteKey,string location)=>$"cms:public:{siteKey}:menu:{location}";
    public static string Settings(string siteKey)=>$"cms:public:{siteKey}:settings";
}
