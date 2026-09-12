using CustomCodeFramework.Postgres.EntityFramework.Configurations;
using Dhole.Content.Domain.Forms.Entities;
using Dhole.Content.Domain.Submissions.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dhole.Content.Persistence.Configurations.Submissions;

internal sealed class MarketingSubmissionConfiguration : EntityTypeConfigurationBase<MarketingSubmission, Guid>
{
    public override void Configure(EntityTypeBuilder<MarketingSubmission> builder)
    {
        base.Configure(builder);
        builder.ToTable("marketing_submissions");
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.Status).HasMaxLength(80).IsRequired();
        builder.Property(x => x.SourceUrl).HasMaxLength(2048).IsRequired();
        builder.Property(x => x.ReferrerUrl).HasMaxLength(2048);
        builder.Property(x => x.UtmSource).HasMaxLength(240);
        builder.Property(x => x.UtmMedium).HasMaxLength(240);
        builder.Property(x => x.UtmCampaign).HasMaxLength(240);
        builder.Property(x => x.UtmContent).HasMaxLength(240);
        builder.Property(x => x.UtmTerm).HasMaxLength(240);
        builder.Property(x => x.PayloadJson).HasColumnType("jsonb").IsRequired();
        builder.Property(x => x.IpHash).HasMaxLength(64);
        builder.Property(x => x.UserAgent).HasMaxLength(1024);
        builder.HasIndex(x => new { x.FormId, x.SubmittedAtUtc }).HasFilter("is_deleted = false");
        builder.HasIndex(x => x.CorrelationId).IsUnique().HasFilter("is_deleted = false");
        builder.HasIndex(x => x.CampaignId).HasFilter("campaign_id IS NOT NULL AND is_deleted = false");
        builder.HasOne<MarketingForm>().WithMany().HasForeignKey(x => x.FormId).OnDelete(DeleteBehavior.Restrict);
    }
}
