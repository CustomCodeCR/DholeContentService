using Dhole.Content.Domain.Leads;
using Dhole.Content.Domain.Leads.Entities;
using Dhole.Content.Domain.Leads.Events;
using Dhole.Content.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Dhole.Content.UnitTests;

public sealed class Phase17MarketingLeadTests
{
    [Fact]
    public void MarketingLead_Create_NormalizesContactAndDefaultsStatus()
    {
        var now = DateTime.UtcNow;
        var lead = MarketingLead.Create(" MAIN ", " Ana ", " Mora ", " ANA@EXAMPLE.COM ", null,
            " GCF ", " Gerente ", " Costa Rica ", " website ", null, null, null, null, null, now);

        Assert.Equal("main", lead.SiteKey);
        Assert.Equal("Ana", lead.FirstName);
        Assert.Equal("ana@example.com", lead.Email);
        Assert.Equal(MarketingLeadRules.DefaultStatus, lead.Status);
        Assert.Equal(now, lead.FirstTouchAtUtc);
        Assert.Equal(now, lead.LastTouchAtUtc);
        Assert.Contains(lead.DomainEvents, item => item is MarketingLeadCreatedDomainEvent);
    }

    [Fact]
    public void MarketingLead_Create_RequiresEmailOrPhone()
        => Assert.Throws<ArgumentException>(() => MarketingLead.Create("main", "Ana", null, null, null,
            null, null, null, "website", null, null, null, null, null, DateTime.UtcNow));

    [Fact]
    public void MarketingLead_Update_PreservesFirstTouchAndAdvancesLastTouch()
    {
        var first = DateTime.UtcNow.AddHours(-2);
        var lead = MarketingLead.Create("main", "Ana", null, "ana@example.com", null, null, null, null,
            "website", null, null, first, first, null, first);
        var next = first.AddHours(1);

        lead.Update("main", "Ana", "Mora", "ana@example.com", "+506 8888-8888", "GCF", null,
            "Costa Rica", "campaign", "Contacted", Guid.NewGuid(), next, null, next);

        Assert.Equal(first, lead.FirstTouchAtUtc);
        Assert.Equal(next, lead.LastTouchAtUtc);
        Assert.Equal("Contacted", lead.Status);
        Assert.Contains(lead.DomainEvents, item => item is MarketingLeadUpdatedDomainEvent);
    }

    [Fact]
    public void MarketingLead_Update_RejectsBackwardLastTouch()
    {
        var first = DateTime.UtcNow;
        var lead = MarketingLead.Create("main", null, null, "ana@example.com", null, null, null, null,
            null, null, null, first, first, null, first);

        Assert.Throws<ArgumentException>(() => lead.Update("main", null, null, "ana@example.com", null,
            null, null, null, null, null, null, first.AddMinutes(-1), null, first.AddMinutes(1)));
    }

    [Fact]
    public void MarketingLeadModel_HasExpectedIndexesAndNoCrossServiceOwnerForeignKey()
    {
        var options = new DbContextOptionsBuilder<ServiceDbContext>()
            .UseNpgsql("Host=localhost;Database=dhole_content_phase17_test")
            .Options;
        using var db = new ServiceDbContext(options);

        var lead = db.Model.FindEntityType(typeof(MarketingLead));
        Assert.NotNull(lead);

        var uniqueEmail = lead!.GetIndexes().Single(index =>
            index.Properties.Select(property => property.Name).SequenceEqual(["SiteKey", "Email"]));
        Assert.True(uniqueEmail.IsUnique);
        Assert.Equal("email IS NOT NULL AND is_deleted = false", uniqueEmail.GetFilter());

        var statusIndex = lead.GetIndexes().Single(index =>
            index.Properties.Select(property => property.Name).SequenceEqual(["SiteKey", "Status"]));
        Assert.False(statusIndex.IsUnique);

        Assert.DoesNotContain(lead.GetForeignKeys(), foreignKey =>
            foreignKey.Properties.Any(property => property.Name == "OwnerUserId"));
    }

    [Fact]
    public void ServiceDbContext_ExposesMarketingLeads()
    {
        var propertyNames = typeof(ServiceDbContext).GetProperties()
            .Select(property => property.Name)
            .ToHashSet(StringComparer.Ordinal);
        Assert.Contains("MarketingLeads", propertyNames);
    }
}
