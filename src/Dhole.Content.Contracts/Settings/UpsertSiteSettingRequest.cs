namespace Dhole.Content.Contracts.Settings;
public sealed record UpsertSiteSettingRequest(string Key,string ValueJson,bool IsPublic,string? SiteKey);
