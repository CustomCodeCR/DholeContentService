namespace Dhole.Content.Contracts.Leads;

public sealed record MarketingLeadDto(
    Guid Id,
    string SiteKey,
    string? FirstName,
    string? LastName,
    string? Email,
    string? Phone,
    string? Company,
    string? JobTitle,
    string? Country,
    string? Source,
    string Status,
    Guid? OwnerUserId,
    DateTime FirstTouchAtUtc,
    DateTime LastTouchAtUtc,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);

public sealed record CreateMarketingLeadRequest(
    string SiteKey,
    string? FirstName,
    string? LastName,
    string? Email,
    string? Phone,
    string? Company,
    string? JobTitle,
    string? Country,
    string? Source,
    string? Status,
    Guid? OwnerUserId,
    DateTime? FirstTouchAtUtc,
    DateTime? LastTouchAtUtc);

public sealed record UpdateMarketingLeadRequest(
    string SiteKey,
    string? FirstName,
    string? LastName,
    string? Email,
    string? Phone,
    string? Company,
    string? JobTitle,
    string? Country,
    string? Source,
    string? Status,
    Guid? OwnerUserId,
    DateTime? LastTouchAtUtc);

public sealed record TouchMarketingLeadRequest(string? Source);
