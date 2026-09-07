using CustomCodeFramework.Core.Domain.Entities;
using Dhole.Content.Domain.Settings.Events;
namespace Dhole.Content.Domain.Settings.Entities;

public sealed class SiteSetting : SoftDeletableAggregateRoot<Guid>
{
    private SiteSetting() { }
    private SiteSetting(Guid id,string key,string valueJson,bool isPublic,string siteKey,Guid? actor):base(id)
    { Key=key.Trim(); ValueJson=valueJson; IsPublic=isPublic; SiteKey=siteKey.Trim(); MarkAsCreated(DateTime.UtcNow,actor?.ToString()); }
    public string SiteKey { get; private set; }="main";
    public string Key { get; private set; }=string.Empty;
    public string ValueJson { get; private set; }="null";
    public bool IsPublic { get; private set; }
    public static SiteSetting Create(string key,string valueJson,bool isPublic,string? siteKey,Guid? actor)
    { var s=new SiteSetting(Guid.NewGuid(),key,valueJson,isPublic,string.IsNullOrWhiteSpace(siteKey)?"main":siteKey,actor); s.AddDomainEvent(new SiteSettingChangedDomainEvent(s.Id,s.SiteKey,s.Key,s.IsPublic,"created",actor)); return s; }
    public void Update(string valueJson,bool isPublic,Guid? actor){ValueJson=valueJson;IsPublic=isPublic;MarkAsUpdated(DateTime.UtcNow,actor?.ToString());AddDomainEvent(new SiteSettingChangedDomainEvent(Id,SiteKey,Key,IsPublic,"updated",actor));}
    public void Delete(Guid? actor){MarkAsDeleted(DateTime.UtcNow,actor?.ToString());AddDomainEvent(new SiteSettingChangedDomainEvent(Id,SiteKey,Key,IsPublic,"deleted",actor));}
}
