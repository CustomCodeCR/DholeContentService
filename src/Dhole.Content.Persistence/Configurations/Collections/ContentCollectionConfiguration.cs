using CustomCodeFramework.Postgres.EntityFramework.Configurations;
using Dhole.Content.Domain.Collections.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dhole.Content.Persistence.Configurations.Collections;

internal sealed class ContentCollectionConfiguration : EntityTypeConfigurationBase<ContentCollection, Guid>
{
    public override void Configure(EntityTypeBuilder<ContentCollection> builder)
    {
        base.Configure(builder);
        builder.ToTable("collections");
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.SiteKey).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Code).HasMaxLength(160).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(240).IsRequired();
        builder.Property(x => x.SettingsJson).HasColumnType("jsonb");
        builder.HasIndex(x => new { x.SiteKey, x.Code }).IsUnique().HasFilter("is_deleted = false");
    }
}
