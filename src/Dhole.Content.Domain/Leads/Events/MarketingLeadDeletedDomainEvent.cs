using CustomCodeFramework.Core.Domain.Events;

namespace Dhole.Content.Domain.Leads.Events;

public sealed record MarketingLeadDeletedDomainEvent(Guid id, string siteKey, string? email, string status) : DomainEvent;
