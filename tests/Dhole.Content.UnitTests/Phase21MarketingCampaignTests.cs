using Dhole.Content.Domain.Campaigns.Entities;
using Dhole.Content.Domain.ContentItems.Entities;
using Dhole.Content.Domain.Submissions.Entities;
using Dhole.Content.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Dhole.Content.UnitTests;

public sealed class Phase21MarketingCampaignTests
{
    [Fact]
    public void Campaign_NormalizesCoreFieldsAndUtm()
    {
        var campaign = MarketingCampaign.Create(" MAIN ", " Lanzamiento ", " Promo.Septiembre ", "Draft",
            new DateTime(2026, 9, 15, 12, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 9, 30, 12, 0, 0, DateTimeKind.Utc), null,
            " newsletter ", " email ", " septiembre ", "lead-generation", "{}", null);

        Assert.Equal("main", campaign.SiteKey);
        Assert.Equal("promo.septiembre", campaign.Slug);
        Assert.Equal("newsletter", campaign.UtmSource);
        Assert.Equal("lead-generation", campaign.GoalType);
    }

    [Fact]
    public void Campaign_RejectsInvalidWindow()
    {
        var start = new DateTime(2026, 9, 20, 12, 0, 0, DateTimeKind.Utc);
        Assert.Throws<ArgumentException>(() => MarketingCampaign.Create("main", "Campaña", "campana", "Draft",
            start, start, null, null, null, null, "leads", null, null));
    }

    [Fact]
    public void Campaign_RejectsNonObjectSettings()
        => Assert.Throws<ArgumentException>(() => MarketingCampaign.Create("main", "Campaña", "campana", "Draft",
            null, null, null, null, null, null, "leads", "[]", null));

    [Fact]
    public void EfModel_HasCampaignSlugUniquenessAndSubmissionRelationship()
    {
        var options = new DbContextOptionsBuilder<ServiceDbContext>().UseNpgsql("Host=localhost;Database=test;Username=test;Password=test").Options;
        using var db = new ServiceDbContext(options);
        var campaign = db.Model.FindEntityType(typeof(MarketingCampaign));
        Assert.NotNull(campaign);
        Assert.Contains(campaign!.GetIndexes(), index => index.IsUnique &&
            index.Properties.Select(x => x.Name).SequenceEqual([nameof(MarketingCampaign.SiteKey), nameof(MarketingCampaign.Slug)]));

        var submission = db.Model.FindEntityType(typeof(MarketingSubmission));
        Assert.NotNull(submission);
        Assert.Contains(submission!.GetForeignKeys(), fk => fk.PrincipalEntityType.ClrType == typeof(MarketingCampaign) &&
            fk.Properties.Single().Name == nameof(MarketingSubmission.CampaignId));

        Assert.Contains(campaign.GetForeignKeys(), fk => fk.PrincipalEntityType.ClrType == typeof(ContentItem) &&
            fk.Properties.Single().Name == nameof(MarketingCampaign.LandingContentId));
    }
}
