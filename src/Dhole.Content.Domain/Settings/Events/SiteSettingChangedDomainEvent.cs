using CustomCodeFramework.Core.Domain.Events;
namespace Dhole.Content.Domain.Settings.Events;
public sealed record SiteSettingChangedDomainEvent(Guid id,string siteKey,string key,bool isPublic,string action,Guid? actorUserId) : DomainEvent;
