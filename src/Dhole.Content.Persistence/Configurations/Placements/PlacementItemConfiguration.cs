using CustomCodeFramework.Postgres.EntityFramework.Configurations;
using Dhole.Content.Domain.ContentItems.Entities;
using Dhole.Content.Domain.Placements.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dhole.Content.Persistence.Configurations.Placements;

internal sealed class PlacementItemConfiguration : EntityTypeConfigurationBase<PlacementItem, Guid>
{
    public override void Configure(EntityTypeBuilder<PlacementItem> builder)
    {
        base.Configure(builder);
        builder.ToTable("placement_items");
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.SettingsJson).HasColumnType("jsonb");
        builder.HasIndex(x => new { x.PlacementId, x.ContentId }).IsUnique().HasFilter("is_deleted = false");
        builder.HasIndex(x => new { x.PlacementId, x.SortOrder }).HasFilter("is_deleted = false");
        builder.HasOne<Placement>().WithMany().HasForeignKey(x => x.PlacementId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<ContentItem>().WithMany().HasForeignKey(x => x.ContentId).OnDelete(DeleteBehavior.Cascade);
    }
}
