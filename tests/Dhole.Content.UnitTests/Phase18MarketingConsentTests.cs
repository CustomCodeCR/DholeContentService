using Dhole.Content.Contracts.Consents;
using Dhole.Content.Domain.Consents;
using Dhole.Content.Domain.Consents.Entities;
using Dhole.Content.Domain.Leads.Entities;
using Dhole.Content.Domain.Submissions.Entities;
using Dhole.Content.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Dhole.Content.UnitTests;

public sealed class Phase18MarketingConsentTests
{
    [Fact]
    public void MarketingConsent_Create_NormalizesPurposePolicyAndSource()
    {
        var now = DateTime.UtcNow;
        var submissionId = Guid.NewGuid();
        var consent = MarketingConsent.Create(null, submissionId, " PRIVACIDAD ", true,
            " 2026-09 ", " Website ", now.AddMinutes(-1), null, now);

        Assert.Equal(MarketingConsentRules.PurposePrivacy, consent.Purpose);
        Assert.True(consent.Granted);
        Assert.Equal("2026-09", consent.PolicyVersion);
        Assert.Equal("website", consent.Source);
        Assert.Equal(submissionId, consent.SubmissionId);
    }

    [Fact]
    public void MarketingConsent_Create_RequiresLeadOrSubmission()
        => Assert.Throws<ArgumentException>(() => MarketingConsent.Create(null, null,
            MarketingConsentRules.PurposeContact, true, "v1", "manual", null, null, DateTime.UtcNow));

    [Fact]
    public void MarketingConsent_Create_RejectsUnsupportedPurpose()
        => Assert.Throws<ArgumentException>(() => MarketingConsent.Create(Guid.NewGuid(), null,
            "profiling", true, "v1", "manual", null, null, DateTime.UtcNow));

    [Fact]
    public void MarketingConsent_Create_RejectsFutureCapture()
    {
        var now = DateTime.UtcNow;
        Assert.Throws<ArgumentException>(() => MarketingConsent.Create(Guid.NewGuid(), null,
            MarketingConsentRules.PurposeNewsletter, false, "v1", "manual", now.AddMinutes(1), null, now));
    }

    [Fact]
    public void MarketingConsentRules_ExposeAllRequiredPurposes()
    {
        Assert.Contains(MarketingConsentRules.PurposePrivacy, MarketingConsentRules.SupportedPurposes);
        Assert.Contains(MarketingConsentRules.PurposeContact, MarketingConsentRules.SupportedPurposes);
        Assert.Contains(MarketingConsentRules.PurposeNewsletter, MarketingConsentRules.SupportedPurposes);
        Assert.Contains(MarketingConsentRules.PurposeCommercialCommunications, MarketingConsentRules.SupportedPurposes);
    }

    [Fact]
    public void SubmitMarketingConsentRequest_AllowsFormSourceToBeOmitted()
    {
        var request = new SubmitMarketingConsentRequest(MarketingConsentRules.PurposePrivacy, true, "v1");
        Assert.Null(request.Source);
    }

    [Fact]
    public void MarketingConsentModel_HasExpectedIndexesAndRestrictiveForeignKeys()
    {
        var options = new DbContextOptionsBuilder<ServiceDbContext>()
            .UseNpgsql("Host=localhost;Database=dhole_content_phase18_test")
            .Options;
        using var db = new ServiceDbContext(options);

        var consent = db.Model.FindEntityType(typeof(MarketingConsent));
        Assert.NotNull(consent);

        var leadIndex = consent!.GetIndexes().Single(index =>
            index.Properties.Select(property => property.Name)
                .SequenceEqual(["LeadId", "Purpose", "CapturedAtUtc"]));
        Assert.Equal("lead_id IS NOT NULL AND is_deleted = false", leadIndex.GetFilter());

        var submissionIndex = consent.GetIndexes().Single(index =>
            index.Properties.Select(property => property.Name)
                .SequenceEqual(["SubmissionId", "Purpose", "CapturedAtUtc"]));
        Assert.Equal("submission_id IS NOT NULL AND is_deleted = false", submissionIndex.GetFilter());

        var leadForeignKey = consent.GetForeignKeys().Single(fk => fk.PrincipalEntityType.ClrType == typeof(MarketingLead));
        var submissionForeignKey = consent.GetForeignKeys().Single(fk => fk.PrincipalEntityType.ClrType == typeof(MarketingSubmission));
        Assert.Equal(DeleteBehavior.Restrict, leadForeignKey.DeleteBehavior);
        Assert.Equal(DeleteBehavior.Restrict, submissionForeignKey.DeleteBehavior);
    }

    [Fact]
    public void ServiceDbContext_ExposesMarketingConsents()
    {
        var propertyNames = typeof(ServiceDbContext).GetProperties()
            .Select(property => property.Name)
            .ToHashSet(StringComparer.Ordinal);
        Assert.Contains("MarketingConsents", propertyNames);
    }
}
