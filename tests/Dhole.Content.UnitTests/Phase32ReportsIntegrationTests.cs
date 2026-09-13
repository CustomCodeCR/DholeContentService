using Dhole.Content.Application.Analytics;
using Dhole.Content.Application.Meetings;
using Xunit;

namespace Dhole.Content.UnitTests;

public sealed class Phase32ReportsIntegrationTests
{
    [Fact]
    public void AnalyticsEventNames_AreStableAndSeparatedByPurpose()
    {
        Assert.Equal("content.analytics.page-viewed", ContentAnalyticsIntegration.PageViewedEventName);
        Assert.Equal("content.analytics.form-submitted", ContentAnalyticsIntegration.FormSubmittedEventName);
        Assert.Equal("content.analytics.interaction-clicked", ContentAnalyticsIntegration.InteractionClickedEventName);
        Assert.Equal("content.meeting.requested", MeetingNotificationIntegration.RequestedEventName);
    }

    [Fact]
    public void PublicAnalyticsPayloads_KeepCampaignAttributionDimensions()
    {
        var pageProperties = typeof(PageViewedAnalyticsEvent).GetProperties().Select(x => x.Name).ToHashSet();
        var formProperties = typeof(FormSubmittedAnalyticsEvent).GetProperties().Select(x => x.Name).ToHashSet();
        var clickProperties = typeof(InteractionClickedAnalyticsEvent).GetProperties().Select(x => x.Name).ToHashSet();

        foreach (var property in new[] { "CampaignId", "UtmSource", "UtmMedium", "UtmCampaign", "UtmContent", "UtmTerm" })
        {
            Assert.Contains(property, pageProperties);
            Assert.Contains(property, formProperties);
            Assert.Contains(property, clickProperties);
        }
    }

    [Fact]
    public void TrackingCommands_DoNotCreateReportingDomainModelsInsideContentService()
    {
        Assert.Contains(typeof(TrackPageViewCommand).GetInterfaces(), type => type.Name.StartsWith("ICommand", StringComparison.Ordinal));
        Assert.Contains(typeof(TrackInteractionClickCommand).GetInterfaces(), type => type.Name.StartsWith("ICommand", StringComparison.Ordinal));
    }
}
