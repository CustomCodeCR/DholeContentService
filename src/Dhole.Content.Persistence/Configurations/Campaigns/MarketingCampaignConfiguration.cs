using CustomCodeFramework.Postgres.EntityFramework.Configurations;
using Dhole.Content.Domain.Campaigns.Entities;
using Dhole.Content.Domain.ContentItems.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dhole.Content.Persistence.Configurations.Campaigns;

internal sealed class MarketingCampaignConfiguration : EntityTypeConfigurationBase<MarketingCampaign, Guid>
{
    public override void Configure(EntityTypeBuilder<MarketingCampaign> builder)
    {
        base.Configure(builder);
        builder.ToTable("campaigns");
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.SiteKey).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(240).IsRequired();
        builder.Property(x => x.Slug).HasMaxLength(160).IsRequired();
        builder.Property(x => x.Status).HasMaxLength(80).IsRequired();
        builder.Property(x => x.UtmSource).HasMaxLength(240);
        builder.Property(x => x.UtmMedium).HasMaxLength(240);
        builder.Property(x => x.UtmCampaign).HasMaxLength(240);
        builder.Property(x => x.GoalType).HasMaxLength(120).IsRequired();
        builder.Property(x => x.SettingsJson).HasColumnType("jsonb");
        builder.HasIndex(x => new { x.SiteKey, x.Slug }).IsUnique().HasFilter("is_deleted = false");
        builder.HasIndex(x => new { x.SiteKey, x.Status }).HasFilter("is_deleted = false");
        builder.HasIndex(x => new { x.StartsAtUtc, x.EndsAtUtc }).HasFilter("is_deleted = false");
        builder.HasOne<ContentItem>().WithMany().HasForeignKey(x => x.LandingContentId).OnDelete(DeleteBehavior.SetNull);
    }
}
