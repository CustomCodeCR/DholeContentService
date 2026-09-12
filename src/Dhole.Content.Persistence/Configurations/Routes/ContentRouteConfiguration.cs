using CustomCodeFramework.Postgres.EntityFramework.Configurations;
using Dhole.Content.Domain.ContentItems.Entities;
using Dhole.Content.Domain.Routes.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dhole.Content.Persistence.Configurations.Routes;

internal sealed class ContentRouteConfiguration : EntityTypeConfigurationBase<ContentRoute, Guid>
{
    public override void Configure(EntityTypeBuilder<ContentRoute> builder)
    {
        base.Configure(builder);
        builder.ToTable("content_routes");
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.SiteKey).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Locale).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Path).HasMaxLength(1200).IsRequired();

        builder.HasIndex(x => new { x.SiteKey, x.Locale, x.Path })
            .IsUnique();

        builder.HasIndex(x => new { x.ContentId, x.Locale })
            .IsUnique()
            .HasFilter("is_primary = true");

        builder.HasIndex(x => new { x.SiteKey, x.ContentId, x.Locale });

        builder.HasOne<ContentItem>()
            .WithMany()
            .HasForeignKey(x => x.ContentId)
            .OnDelete(DeleteBehavior.Cascade);

        // SiteKey is validated against content.sites in the application layer.
        // content.sites uses a partial unique index because it is soft-deletable,
        // and PostgreSQL cannot target a partial unique index with a foreign key.
    }
}
