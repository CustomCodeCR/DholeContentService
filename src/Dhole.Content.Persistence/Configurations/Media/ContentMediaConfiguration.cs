using CustomCodeFramework.Postgres.EntityFramework.Configurations;
using Dhole.Content.Domain.ContentItems.Entities;
using Dhole.Content.Domain.Media.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dhole.Content.Persistence.Configurations.Media;

internal sealed class ContentMediaConfiguration : EntityTypeConfigurationBase<ContentMedia, Guid>
{
    public override void Configure(EntityTypeBuilder<ContentMedia> builder)
    {
        base.Configure(builder);
        builder.ToTable("content_media");
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.Role).HasMaxLength(80).IsRequired();
        builder.Property(x => x.AltTextOverride).HasMaxLength(600);
        builder.Property(x => x.CaptionOverride).HasMaxLength(1200);
        builder.Property(x => x.FocalX).HasPrecision(5, 4);
        builder.Property(x => x.FocalY).HasPrecision(5, 4);
        builder.Property(x => x.SettingsJson).HasColumnType("jsonb");

        builder.HasIndex(x => new { x.ContentId, x.MediaReferenceId, x.Role })
            .IsUnique()
            .HasFilter("is_deleted = false");

        builder.HasIndex(x => new { x.ContentId, x.Role, x.SortOrder })
            .HasFilter("is_deleted = false");

        builder.HasOne<ContentItem>()
            .WithMany()
            .HasForeignKey(x => x.ContentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<MediaReference>()
            .WithMany()
            .HasForeignKey(x => x.MediaReferenceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
