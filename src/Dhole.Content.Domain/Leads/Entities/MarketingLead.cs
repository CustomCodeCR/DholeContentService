using CustomCodeFramework.Core.Domain.Entities;
using Dhole.Content.Domain.Leads.Events;

namespace Dhole.Content.Domain.Leads.Entities;

public sealed class MarketingLead : SoftDeletableAggregateRoot<Guid>
{
    private MarketingLead() { }

    private MarketingLead(
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
        string? status,
        Guid? ownerUserId,
        DateTime firstTouchAtUtc,
        DateTime lastTouchAtUtc,
        Guid? actorUserId,
        DateTime utcNow) : base(id)
    {
        Apply(siteKey, firstName, lastName, email, phone, company, jobTitle, country, source, status, ownerUserId);
        if (lastTouchAtUtc < firstTouchAtUtc)
            throw new ArgumentException("LastTouchAtUtc cannot be earlier than FirstTouchAtUtc.", nameof(lastTouchAtUtc));
        FirstTouchAtUtc = firstTouchAtUtc;
        LastTouchAtUtc = lastTouchAtUtc;
        MarkAsCreated(utcNow, actorUserId?.ToString());
    }

    public string SiteKey { get; private set; } = "main";
    public string? FirstName { get; private set; }
    public string? LastName { get; private set; }
    public string? Email { get; private set; }
    public string? Phone { get; private set; }
    public string? Company { get; private set; }
    public string? JobTitle { get; private set; }
    public string? Country { get; private set; }
    public string? Source { get; private set; }
    public string Status { get; private set; } = MarketingLeadRules.DefaultStatus;
    public Guid? OwnerUserId { get; private set; }
    public DateTime FirstTouchAtUtc { get; private set; }
    public DateTime LastTouchAtUtc { get; private set; }

    public static MarketingLead Create(
        string siteKey,
        string? firstName,
        string? lastName,
        string? email,
        string? phone,
        string? company,
        string? jobTitle,
        string? country,
        string? source,
        string? status,
        Guid? ownerUserId,
        DateTime? firstTouchAtUtc,
        DateTime? lastTouchAtUtc,
        Guid? actorUserId,
        DateTime utcNow)
    {
        var first = firstTouchAtUtc ?? utcNow;
        var last = lastTouchAtUtc ?? first;
        var lead = new MarketingLead(Guid.NewGuid(), siteKey, firstName, lastName, email, phone, company,
            jobTitle, country, source, status, ownerUserId, first, last, actorUserId, utcNow);
        lead.AddDomainEvent(lead.ToCreatedEvent());
        return lead;
    }

    public void Update(
        string siteKey,
        string? firstName,
        string? lastName,
        string? email,
        string? phone,
        string? company,
        string? jobTitle,
        string? country,
        string? source,
        string? status,
        Guid? ownerUserId,
        DateTime? lastTouchAtUtc,
        Guid? actorUserId,
        DateTime utcNow)
    {
        Apply(siteKey, firstName, lastName, email, phone, company, jobTitle, country, source, status, ownerUserId);
        var newLastTouch = lastTouchAtUtc ?? utcNow;
        if (newLastTouch < FirstTouchAtUtc || newLastTouch < LastTouchAtUtc)
            throw new ArgumentException("LastTouchAtUtc cannot move backwards.", nameof(lastTouchAtUtc));
        LastTouchAtUtc = newLastTouch;
        MarkAsUpdated(utcNow, actorUserId?.ToString());
        AddDomainEvent(ToUpdatedEvent());
    }

    public void Touch(string? source, DateTime utcNow, Guid? actorUserId)
    {
        Source = MarketingLeadRules.NormalizeOptionalText(source, 160) ?? Source;
        if (utcNow < LastTouchAtUtc) throw new ArgumentException("Touch time cannot move backwards.", nameof(utcNow));
        LastTouchAtUtc = utcNow;
        MarkAsUpdated(utcNow, actorUserId?.ToString());
        AddDomainEvent(ToUpdatedEvent());
    }

    public void Delete(Guid? actorUserId, DateTime utcNow)
    {
        MarkAsDeleted(utcNow, actorUserId?.ToString());
        AddDomainEvent(new MarketingLeadDeletedDomainEvent(Id, SiteKey, Email, Status));
    }

    private void Apply(
        string siteKey,
        string? firstName,
        string? lastName,
        string? email,
        string? phone,
        string? company,
        string? jobTitle,
        string? country,
        string? source,
        string? status,
        Guid? ownerUserId)
    {
        SiteKey = MarketingLeadRules.NormalizeSiteKey(siteKey);
        FirstName = MarketingLeadRules.NormalizeOptionalText(firstName, 160);
        LastName = MarketingLeadRules.NormalizeOptionalText(lastName, 160);
        Email = MarketingLeadRules.NormalizeEmail(email);
        Phone = MarketingLeadRules.NormalizeOptionalText(phone, 60);
        Company = MarketingLeadRules.NormalizeOptionalText(company, 240);
        JobTitle = MarketingLeadRules.NormalizeOptionalText(jobTitle, 160);
        Country = MarketingLeadRules.NormalizeOptionalText(country, 120);
        Source = MarketingLeadRules.NormalizeOptionalText(source, 160);
        Status = MarketingLeadRules.NormalizeStatus(status);
        OwnerUserId = ownerUserId;
        if (Email is null && Phone is null)
            throw new ArgumentException("A marketing lead requires at least email or phone.");
    }

    private MarketingLeadCreatedDomainEvent ToCreatedEvent()
        => new(Id, SiteKey, FirstName, LastName, Email, Phone, Company, JobTitle, Country, Source,
            Status, OwnerUserId, FirstTouchAtUtc, LastTouchAtUtc);

    private MarketingLeadUpdatedDomainEvent ToUpdatedEvent()
        => new(Id, SiteKey, FirstName, LastName, Email, Phone, Company, JobTitle, Country, Source,
            Status, OwnerUserId, FirstTouchAtUtc, LastTouchAtUtc);
}
