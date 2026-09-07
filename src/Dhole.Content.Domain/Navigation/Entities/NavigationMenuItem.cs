namespace Dhole.Content.Domain.Navigation.Entities;
public sealed class NavigationMenuItem
{
    private NavigationMenuItem() { }
    public Guid Id { get; private set; }
    public Guid MenuId { get; private set; }
    public Guid? ParentId { get; private set; }
    public string Label { get; private set; }=string.Empty;
    public string? Url { get; private set; }
    public Guid? ContentId { get; private set; }
    public string Target { get; private set; }="_self";
    public int SortOrder { get; private set; }
    public bool IsVisible { get; private set; }=true;
    public NavigationMenu Menu { get; private set; }=default!;
    internal static NavigationMenuItem Create(Guid menuId,string label,string? url,Guid? contentId,Guid? parentId,int sortOrder,string? target)
      => new(){Id=Guid.NewGuid(),MenuId=menuId,Label=label.Trim(),Url=string.IsNullOrWhiteSpace(url)?null:url.Trim(),ContentId=contentId,ParentId=parentId,SortOrder=sortOrder,Target=string.IsNullOrWhiteSpace(target)?"_self":target.Trim()};
}
