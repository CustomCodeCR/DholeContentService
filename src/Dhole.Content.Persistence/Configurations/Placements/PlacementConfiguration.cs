using CustomCodeFramework.Postgres.EntityFramework.Configurations;
using Dhole.Content.Domain.Placements.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dhole.Content.Persistence.Configurations.Placements;

internal sealed class PlacementConfiguration : EntityTypeConfigurationBase<Placement, Guid>
{
    public override void Configure(EntityTypeBuilder<Placement> builder)
    {
        base.Configure(builder);
        builder.ToTable("placements");
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.SiteKey).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Code).HasMaxLength(160).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(240).IsRequired();
        builder.Property(x => x.AllowedTypesJson).HasColumnType("jsonb").IsRequired();
        builder.Property(x => x.SettingsJson).HasColumnType("jsonb");
        builder.HasIndex(x => new { x.SiteKey, x.Code }).IsUnique().HasFilter("is_deleted = false");
    }
}
