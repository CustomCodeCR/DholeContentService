using CustomCodeFramework.Postgres.EntityFramework.Configurations;
using Dhole.Content.Domain.Consents.Entities;
using Dhole.Content.Domain.Leads.Entities;
using Dhole.Content.Domain.Submissions.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dhole.Content.Persistence.Configurations.Consents;

internal sealed class MarketingConsentConfiguration : EntityTypeConfigurationBase<MarketingConsent, Guid>
{
    public override void Configure(EntityTypeBuilder<MarketingConsent> builder)
    {
        base.Configure(builder);
        builder.ToTable("marketing_consents");
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.Purpose).HasMaxLength(80).IsRequired();
        builder.Property(x => x.PolicyVersion).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Source).HasMaxLength(160).IsRequired();
        builder.HasIndex(x => new { x.LeadId, x.Purpose, x.CapturedAtUtc })
            .HasFilter("lead_id IS NOT NULL AND is_deleted = false");
        builder.HasIndex(x => new { x.SubmissionId, x.Purpose, x.CapturedAtUtc })
            .HasFilter("submission_id IS NOT NULL AND is_deleted = false");
        builder.HasIndex(x => new { x.Purpose, x.CapturedAtUtc }).HasFilter("is_deleted = false");
        builder.HasOne<MarketingLead>().WithMany().HasForeignKey(x => x.LeadId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MarketingSubmission>().WithMany().HasForeignKey(x => x.SubmissionId).OnDelete(DeleteBehavior.Restrict);
    }
}
