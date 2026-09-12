using CustomCodeFramework.Postgres.EntityFramework.Configurations;
using Dhole.Content.Domain.Meetings.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dhole.Content.Persistence.Configurations.Meetings;

internal sealed class MeetingTypeConfiguration : EntityTypeConfigurationBase<MeetingType, Guid>
{
    public override void Configure(EntityTypeBuilder<MeetingType> builder)
    {
        base.Configure(builder);
        builder.ToTable("meeting_types");
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.SiteKey).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(240).IsRequired();
        builder.Property(x => x.Slug).HasMaxLength(160).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(4000);
        builder.Property(x => x.MeetingMode).HasMaxLength(80).IsRequired();
        builder.Property(x => x.AssignedTeamKey).HasMaxLength(160);
        builder.Property(x => x.SettingsJson).HasColumnType("jsonb");
        builder.HasIndex(x => new { x.SiteKey, x.Slug }).IsUnique().HasFilter("is_deleted = false");
        builder.HasIndex(x => new { x.SiteKey, x.IsActive }).HasFilter("is_deleted = false");
    }
}
