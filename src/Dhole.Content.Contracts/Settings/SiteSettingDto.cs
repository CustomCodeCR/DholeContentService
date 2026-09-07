namespace Dhole.Content.Contracts.Settings;
public sealed record SiteSettingDto(Guid Id,string SiteKey,string Key,string ValueJson,bool IsPublic,DateTime CreatedAtUtc,DateTime? UpdatedAtUtc);
