namespace Dhole.Content.Contracts.Navigation;
public sealed record NavigationMenuDto(Guid Id,string SiteKey,string Name,string Location,bool IsActive,IReadOnlyCollection<NavigationMenuItemDto> Items,DateTime CreatedAtUtc,DateTime? UpdatedAtUtc);
public sealed record NavigationMenuItemDto(Guid Id,Guid? ParentId,string Label,string? Url,Guid? ContentId,string Target,int SortOrder,bool IsVisible);
