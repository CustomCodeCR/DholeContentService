using CustomCodeFramework.Postgres.EntityFramework.Configurations;
using Dhole.Content.Domain.Sites.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dhole.Content.Persistence.Configurations.Sites;

internal sealed class SiteConfiguration : EntityTypeConfigurationBase<Site, Guid>
{
    public override void Configure(EntityTypeBuilder<Site> builder)
    {
        base.Configure(builder);
        builder.ToTable("sites");
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.SiteKey).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.PrimaryDomain).HasMaxLength(255).IsRequired();
        builder.Property(x => x.DefaultLocale).HasMaxLength(20).IsRequired();
        builder.Property(x => x.TimeZone).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Status).HasMaxLength(40).IsRequired();

        builder.HasIndex(x => x.SiteKey)
            .IsUnique()
            .HasFilter("is_deleted = false");

        builder.HasIndex(x => x.PrimaryDomain)
            .IsUnique()
            .HasFilter("is_deleted = false");

        builder.HasOne<Dhole.Content.Domain.Media.Entities.MediaReference>()
            .WithMany()
            .HasForeignKey(x => x.LogoMediaId)
            .OnDelete(DeleteBehavior.SetNull);
        builder.HasOne<Dhole.Content.Domain.Media.Entities.MediaReference>()
            .WithMany()
            .HasForeignKey(x => x.FaviconMediaId)
            .OnDelete(DeleteBehavior.SetNull);
        builder.HasOne<Dhole.Content.Domain.Media.Entities.MediaReference>()
            .WithMany()
            .HasForeignKey(x => x.DefaultOpenGraphMediaId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
