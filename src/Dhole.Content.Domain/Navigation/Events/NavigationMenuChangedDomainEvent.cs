using CustomCodeFramework.Core.Domain.Events;
namespace Dhole.Content.Domain.Navigation.Events;
public sealed record NavigationMenuChangedDomainEvent(Guid id,string siteKey,string location,string name,string action,Guid? actorUserId) : DomainEvent;
