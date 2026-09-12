using CustomCodeFramework.Postgres.EntityFramework.Configurations;
using Dhole.Content.Domain.Leads.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dhole.Content.Persistence.Configurations.Leads;

internal sealed class MarketingLeadConfiguration : EntityTypeConfigurationBase<MarketingLead, Guid>
{
    public override void Configure(EntityTypeBuilder<MarketingLead> builder)
    {
        base.Configure(builder);
        builder.ToTable("marketing_leads");
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.SiteKey).HasMaxLength(80).IsRequired();
        builder.Property(x => x.FirstName).HasMaxLength(160);
        builder.Property(x => x.LastName).HasMaxLength(160);
        builder.Property(x => x.Email).HasMaxLength(320);
        builder.Property(x => x.Phone).HasMaxLength(60);
        builder.Property(x => x.Company).HasMaxLength(240);
        builder.Property(x => x.JobTitle).HasMaxLength(160);
        builder.Property(x => x.Country).HasMaxLength(120);
        builder.Property(x => x.Source).HasMaxLength(160);
        builder.Property(x => x.Status).HasMaxLength(80).IsRequired();
        builder.HasIndex(x => new { x.SiteKey, x.Email })
            .IsUnique()
            .HasFilter("email IS NOT NULL AND is_deleted = false");
        builder.HasIndex(x => new { x.SiteKey, x.Status }).HasFilter("is_deleted = false");
        builder.HasIndex(x => x.OwnerUserId).HasFilter("owner_user_id IS NOT NULL AND is_deleted = false");
        builder.HasIndex(x => x.LastTouchAtUtc).HasFilter("is_deleted = false");
    }
}
