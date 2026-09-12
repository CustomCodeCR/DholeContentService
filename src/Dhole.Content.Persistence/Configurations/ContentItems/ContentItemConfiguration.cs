using CustomCodeFramework.Postgres.EntityFramework.Configurations;
using Dhole.Content.Domain.ContentItems.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dhole.Content.Persistence.Configurations.ContentItems;

internal sealed class ContentItemConfiguration : EntityTypeConfigurationBase<ContentItem, Guid>
{
    public override void Configure(EntityTypeBuilder<ContentItem> b)
    {
        base.Configure(b);
        b.ToTable("content_items");
        b.Property(x => x.Id).ValueGeneratedNever();
        b.Property(x => x.SiteKey).HasMaxLength(80).IsRequired();
        b.Property(x => x.Type).HasConversion<string>().HasMaxLength(40).IsRequired();
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(40).IsRequired();
        b.Property(x => x.Title).HasMaxLength(300).IsRequired();
        b.Property(x => x.Slug).HasMaxLength(300).IsRequired();
        b.Property(x => x.Excerpt).HasMaxLength(1200);
        b.Property(x => x.BlocksJson).HasColumnType("jsonb").IsRequired();
        b.Property(x => x.RenderedHtml).HasColumnType("text");
        b.Property(x => x.Locale).HasMaxLength(20).IsRequired();
        b.Property(x => x.TemplateKey).HasMaxLength(120);
        b.Property(x => x.SitemapPriority).HasPrecision(3, 2);
        b.Property(x => x.SitemapChangeFrequency).HasMaxLength(30);
        b.Property(x => x.SeoTitle).HasMaxLength(300);
        b.Property(x => x.SeoDescription).HasMaxLength(600);
        b.Property(x => x.SeoKeywords).HasMaxLength(1000);
        b.Property(x => x.CanonicalUrl).HasMaxLength(1000);
        b.Property(x => x.Robots).HasMaxLength(120);
        b.Property(x => x.StructuredDataJson).HasColumnType("jsonb");

        b.HasIndex(x => new { x.SiteKey, x.Locale, x.Slug })
            .IsUnique()
            .HasDatabaseName("ux_content_items_site_locale_slug")
            .HasFilter("is_deleted = false");
        b.HasIndex(x => new { x.SiteKey, x.Type, x.Status, x.PublishedAtUtc });
        b.HasIndex(x => new { x.SiteKey, x.TranslationGroupId });
        b.HasIndex(x => x.ParentContentId);
        b.HasIndex(x => x.ScheduledAtUtc)
            .HasDatabaseName("ix_content_items_scheduled_due")
            .HasFilter("is_deleted = false AND status = 'Scheduled' AND scheduled_at_utc IS NOT NULL");
        b.HasIndex(x => x.UnpublishAtUtc)
            .HasDatabaseName("ix_content_items_unpublish_due")
            .HasFilter("is_deleted = false AND status = 'Published' AND unpublish_at_utc IS NOT NULL");

        b.HasOne<ContentItem>()
            .WithMany()
            .HasForeignKey(x => x.ParentContentId)
            .OnDelete(DeleteBehavior.SetNull);

        b.HasMany(x => x.Revisions)
            .WithOne(x => x.Content)
            .HasForeignKey(x => x.ContentId)
            .OnDelete(DeleteBehavior.Cascade);
        b.Navigation(x => x.Revisions).UsePropertyAccessMode(PropertyAccessMode.Field);

        b.HasMany(x => x.Taxonomies)
            .WithOne(x => x.Content)
            .HasForeignKey(x => x.ContentId)
            .OnDelete(DeleteBehavior.Cascade);
        b.Navigation(x => x.Taxonomies).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
