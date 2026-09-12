using CustomCodeFramework.Postgres.EntityFramework.Configurations;
using Dhole.Content.Domain.Forms.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dhole.Content.Persistence.Configurations.Forms;

internal sealed class MarketingFormConfiguration : EntityTypeConfigurationBase<MarketingForm, Guid>
{
    public override void Configure(EntityTypeBuilder<MarketingForm> builder)
    {
        base.Configure(builder);
        builder.ToTable("marketing_forms");
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.SiteKey).HasMaxLength(80).IsRequired();
        builder.Property(x => x.FormKey).HasMaxLength(160).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(240).IsRequired();
        builder.Property(x => x.Purpose).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Status).HasMaxLength(40).IsRequired();
        builder.Property(x => x.SuccessMessage).HasMaxLength(1000);
        builder.Property(x => x.NotificationTemplateKey).HasMaxLength(160);
        builder.Property(x => x.SettingsJson).HasColumnType("jsonb");
        builder.Property(x => x.Version).IsRequired();
        builder.HasIndex(x => new { x.SiteKey, x.FormKey }).IsUnique().HasFilter("is_deleted = false");
        builder.HasIndex(x => new { x.SiteKey, x.Purpose, x.Status }).HasFilter("is_deleted = false");
    }
}
