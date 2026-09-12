using CustomCodeFramework.Core.Domain.Events;

namespace Dhole.Content.Domain.Leads.Events;

public sealed record MarketingLeadCreatedDomainEvent(
    Guid id,
    string siteKey,
    string? firstName,
    string? lastName,
    string? email,
    string? phone,
    string? company,
    string? jobTitle,
    string? country,
    string? source,
    string status,
    Guid? ownerUserId,
    DateTime firstTouchAtUtc,
    DateTime lastTouchAtUtc) : DomainEvent;
