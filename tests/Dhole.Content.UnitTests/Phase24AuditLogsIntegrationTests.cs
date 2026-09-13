using Dhole.Content.Application.Auditing;
using Xunit;

namespace Dhole.Content.UnitTests;

public sealed class Phase24AuditLogsIntegrationTests
{
    [Fact]
    public void AuditLogsContract_MatchesExistingIntegration()
    {
        Assert.Equal("DholeContentService", ContentAuditIntegration.SourceService);
        Assert.Equal("Dhole.AuditLogs.Contracts.AuditEvents.RegisterAuditEventRequest", ContentAuditIntegration.RegisterEventContract);
        Assert.Equal("audit.event.registered", ContentAuditIntegration.RegisteredMessage);
        Assert.Equal("dhole.audit.events", ContentAuditIntegration.RedisStream);
    }

    [Fact]
    public void RequiredPhase24Actions_AreExplicit()
    {
        Assert.Equal("created", ContentAuditActions.Created);
        Assert.Equal("updated", ContentAuditActions.Updated);
        Assert.Equal("published", ContentAuditActions.Published);
        Assert.Equal("approved", ContentAuditActions.Approved);
        Assert.Equal("rejected", ContentAuditActions.Rejected);
        Assert.Equal("deleted", ContentAuditActions.Deleted);
        Assert.Equal("status_changed", ContentAuditActions.StatusChanged);
        Assert.Equal("reordered", ContentAuditActions.Reordered);
    }

    [Theory]
    [InlineData(false, false, "updated")]
    [InlineData(true, false, "status_changed")]
    [InlineData(false, true, "reordered")]
    [InlineData(true, true, "reordered")]
    public void ResolveMutation_PrioritizesReorderThenStatus(bool statusChanged, bool reordered, string expected)
        => Assert.Equal(expected, ContentAuditActions.ResolveMutation(statusChanged, reordered));

    [Fact]
    public void Phase15To20Entities_HaveSemanticAuditContracts()
    {
        Assert.Equal("MarketingSubmission", ContentAuditEntityTypes.MarketingSubmission);
        Assert.Equal("MarketingConsent", ContentAuditEntityTypes.MarketingConsent);
        Assert.Equal("MeetingType", ContentAuditEntityTypes.MeetingType);
        Assert.Equal("MeetingRequest", ContentAuditEntityTypes.MeetingRequest);
        Assert.Equal("content.marketing-submission.created", ContentAuditEventTypes.MarketingSubmissionCreated);
        Assert.Equal("content.marketing-consent.created", ContentAuditEventTypes.MarketingConsentCreated);
        Assert.Equal("content.meeting-request.status-changed", ContentAuditEventTypes.MeetingRequestStatusChanged);
    }
}
