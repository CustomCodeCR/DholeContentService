using CustomCodeFramework.Core.Domain.Entities;
using Dhole.Content.Domain.Navigation.Events;
namespace Dhole.Content.Domain.Navigation.Entities;

public sealed class NavigationMenu : SoftDeletableAggregateRoot<Guid>
{
    private readonly List<NavigationMenuItem> _items=[];
    private NavigationMenu() { }
    private NavigationMenu(Guid id,string name,string location,string siteKey,Guid? actor):base(id)
    { Name=name.Trim(); Location=location.Trim().ToLowerInvariant(); SiteKey=siteKey.Trim(); IsActive=true; MarkAsCreated(DateTime.UtcNow,actor?.ToString()); }
    public string SiteKey { get; private set; }="main";
    public string Name { get; private set; }=string.Empty;
    public string Location { get; private set; }=string.Empty;
    public bool IsActive { get; private set; }
    public IReadOnlyCollection<NavigationMenuItem> Items=>_items;
    public static NavigationMenu Create(string name,string location,string? siteKey,Guid? actor)
    { var menu=new NavigationMenu(Guid.NewGuid(),name,location,string.IsNullOrWhiteSpace(siteKey)?"main":siteKey,actor); menu.AddDomainEvent(new NavigationMenuChangedDomainEvent(menu.Id,menu.SiteKey,menu.Location,menu.Name,"created",actor)); return menu; }
    public void Replace(string name,IEnumerable<(string Label,string? Url,Guid? ContentId,Guid? ParentId,int SortOrder,string? Target)> items,Guid? actor)
    { Name=name.Trim(); _items.Clear(); foreach(var i in items.OrderBy(x=>x.SortOrder)) _items.Add(NavigationMenuItem.Create(Id,i.Label,i.Url,i.ContentId,i.ParentId,i.SortOrder,i.Target)); MarkAsUpdated(DateTime.UtcNow,actor?.ToString()); AddDomainEvent(new NavigationMenuChangedDomainEvent(Id,SiteKey,Location,Name,"updated",actor)); }
    public void SetActive(bool active,Guid? actor){if(IsActive==active)return; IsActive=active; MarkAsUpdated(DateTime.UtcNow,actor?.ToString()); AddDomainEvent(new NavigationMenuChangedDomainEvent(Id,SiteKey,Location,Name,active?"activated":"inactivated",actor));}
    public void Delete(Guid? actor){MarkAsDeleted(DateTime.UtcNow,actor?.ToString()); AddDomainEvent(new NavigationMenuChangedDomainEvent(Id,SiteKey,Location,Name,"deleted",actor));}
}
