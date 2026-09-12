using CustomCodeFramework.Postgres.EntityFramework.Configurations;
using Dhole.Content.Domain.Forms.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dhole.Content.Persistence.Configurations.Forms;

internal sealed class MarketingFormFieldConfiguration : EntityTypeConfigurationBase<MarketingFormField, Guid>
{
    public override void Configure(EntityTypeBuilder<MarketingFormField> builder)
    {
        base.Configure(builder);
        builder.ToTable("marketing_form_fields");
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.FieldKey).HasMaxLength(160).IsRequired();
        builder.Property(x => x.Label).HasMaxLength(240).IsRequired();
        builder.Property(x => x.FieldType).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Placeholder).HasMaxLength(500);
        builder.Property(x => x.ValidationJson).HasColumnType("jsonb");
        builder.Property(x => x.OptionsJson).HasColumnType("jsonb");
        builder.HasIndex(x => new { x.FormId, x.FieldKey }).IsUnique().HasFilter("is_deleted = false");
        builder.HasIndex(x => new { x.FormId, x.SortOrder }).HasFilter("is_deleted = false");
        builder.HasOne<MarketingForm>().WithMany().HasForeignKey(x => x.FormId).OnDelete(DeleteBehavior.Cascade);
    }
}
