namespace Dhole.Content.Contracts.Navigation;
public sealed record UpsertNavigationMenuRequest(string Name,string Location,string? SiteKey,IReadOnlyCollection<NavigationMenuItemWriteRequest> Items);
public sealed record NavigationMenuItemWriteRequest(string Label,string? Url,Guid? ContentId,Guid? ParentId,int SortOrder,string? Target);
