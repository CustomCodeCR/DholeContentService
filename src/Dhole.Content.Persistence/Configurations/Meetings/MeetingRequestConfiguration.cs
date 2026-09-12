using CustomCodeFramework.Postgres.EntityFramework.Configurations;
using Dhole.Content.Domain.Leads.Entities;
using Dhole.Content.Domain.Meetings.Entities;
using Dhole.Content.Domain.Submissions.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dhole.Content.Persistence.Configurations.Meetings;

internal sealed class MeetingRequestConfiguration : EntityTypeConfigurationBase<MeetingRequest, Guid>
{
    public override void Configure(EntityTypeBuilder<MeetingRequest> builder)
    {
        base.Configure(builder);
        builder.ToTable("meeting_requests");
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.TimeZone).HasMaxLength(120).IsRequired();
        builder.Property(x => x.Subject).HasMaxLength(240).IsRequired();
        builder.Property(x => x.Message).HasMaxLength(4000);
        builder.Property(x => x.Status).HasMaxLength(80).IsRequired();
        builder.Property(x => x.ExternalProvider).HasMaxLength(120);
        builder.Property(x => x.ExternalEventId).HasMaxLength(240);
        builder.Property(x => x.MeetingUrl).HasMaxLength(2048);
        builder.HasIndex(x => new { x.MeetingTypeId, x.RequestedStartUtc }).HasFilter("is_deleted = false");
        builder.HasIndex(x => new { x.Status, x.RequestedStartUtc }).HasFilter("is_deleted = false");
        builder.HasIndex(x => x.LeadId).HasFilter("lead_id IS NOT NULL AND is_deleted = false");
        builder.HasIndex(x => x.SubmissionId).HasFilter("submission_id IS NOT NULL AND is_deleted = false");
        builder.HasIndex(x => x.AssignedUserId).HasFilter("assigned_user_id IS NOT NULL AND is_deleted = false");
        builder.HasOne<MeetingType>().WithMany().HasForeignKey(x => x.MeetingTypeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MarketingLead>().WithMany().HasForeignKey(x => x.LeadId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne<MarketingSubmission>().WithMany().HasForeignKey(x => x.SubmissionId).OnDelete(DeleteBehavior.SetNull);
    }
}
