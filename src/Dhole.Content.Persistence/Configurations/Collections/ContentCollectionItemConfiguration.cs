using CustomCodeFramework.Postgres.EntityFramework.Configurations;
using Dhole.Content.Domain.Collections.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dhole.Content.Persistence.Configurations.Collections;

internal sealed class ContentCollectionItemConfiguration : EntityTypeConfigurationBase<ContentCollectionItem, Guid>
{
    public override void Configure(EntityTypeBuilder<ContentCollectionItem> builder)
    {
        base.Configure(builder);
        builder.ToTable("collection_items");
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.DataJson).HasColumnType("jsonb").IsRequired();
        builder.HasIndex(x => new { x.CollectionId, x.SortOrder }).HasFilter("is_deleted = false");
        builder.HasOne<ContentCollection>().WithMany().HasForeignKey(x => x.CollectionId).OnDelete(DeleteBehavior.Cascade);
    }
}
