using CustomCodeFramework.Messaging.Inbox;
using CustomCodeFramework.Messaging.Outbox;
using CustomCodeFramework.Postgres.EntityFramework.Configurations;
using CustomCodeFramework.Postgres.EntityFramework.DbContexts;
using Dhole.Content.Domain.Content;
using Microsoft.EntityFrameworkCore;

namespace Dhole.Content.Persistence.DbContexts;

public sealed class ServiceDbContext(DbContextOptions<ServiceDbContext> options)
    : AppDbContextBase(options)
{
    public DbSet<ContentItem> ContentItems => Set<ContentItem>();
    public DbSet<ContentRevision> ContentRevisions => Set<ContentRevision>();
    public DbSet<TaxonomyTerm> TaxonomyTerms => Set<TaxonomyTerm>();
    public DbSet<ContentTaxonomy> ContentTaxonomies => Set<ContentTaxonomy>();
    public DbSet<MediaReference> MediaReferences => Set<MediaReference>();
    public DbSet<NavigationMenu> NavigationMenus => Set<NavigationMenu>();
    public DbSet<NavigationMenuItem> NavigationMenuItems => Set<NavigationMenuItem>();
    public DbSet<SiteSetting> SiteSettings => Set<SiteSetting>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("content");
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ContentItem>(b =>
        {
            b.ToTable("content_items");
            b.HasKey(x => x.Id);
            b.Property(x => x.SiteKey).HasMaxLength(80).IsRequired();
            b.Property(x => x.Type).HasConversion<string>().HasMaxLength(40);
            b.Property(x => x.Status).HasConversion<string>().HasMaxLength(40);
            b.Property(x => x.Title).HasMaxLength(300).IsRequired();
            b.Property(x => x.Slug).HasMaxLength(300).IsRequired();
            b.Property(x => x.Excerpt).HasMaxLength(1200);
            b.Property(x => x.BlocksJson).HasColumnType("jsonb").IsRequired();
            b.Property(x => x.RenderedHtml).HasColumnType("text");
            b.Property(x => x.Locale).HasMaxLength(20);
            b.Property(x => x.SeoTitle).HasMaxLength(300);
            b.Property(x => x.SeoDescription).HasMaxLength(600);
            b.Property(x => x.SeoKeywords).HasMaxLength(1000);
            b.Property(x => x.CanonicalUrl).HasMaxLength(1000);
            b.Property(x => x.Robots).HasMaxLength(120);
            b.Property(x => x.StructuredDataJson).HasColumnType("jsonb");
            b.HasIndex(x => new { x.SiteKey, x.Slug }).IsUnique().HasFilter("\"DeletedAtUtc\" IS NULL");
            b.HasIndex(x => new { x.SiteKey, x.Type, x.Status, x.PublishedAtUtc });
            b.HasQueryFilter(x => x.DeletedAtUtc == null);
            b.HasMany(x => x.Revisions).WithOne(x => x.Content).HasForeignKey(x => x.ContentId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ContentRevision>(b =>
        {
            b.ToTable("content_revisions");
            b.HasKey(x => x.Id);
            b.Property(x => x.Title).HasMaxLength(300).IsRequired();
            b.Property(x => x.Slug).HasMaxLength(300).IsRequired();
            b.Property(x => x.BlocksJson).HasColumnType("jsonb");
            b.Property(x => x.RenderedHtml).HasColumnType("text");
            b.Property(x => x.StructuredDataJson).HasColumnType("jsonb");
            b.Property(x => x.Reason).HasMaxLength(300);
            b.HasIndex(x => new { x.ContentId, x.RevisionNumber }).IsUnique();
        });

        modelBuilder.Entity<TaxonomyTerm>(b =>
        {
            b.ToTable("taxonomy_terms");
            b.HasKey(x => x.Id);
            b.Property(x => x.SiteKey).HasMaxLength(80);
            b.Property(x => x.Kind).HasMaxLength(30);
            b.Property(x => x.Name).HasMaxLength(200);
            b.Property(x => x.Slug).HasMaxLength(200);
            b.Property(x => x.Description).HasMaxLength(1200);
            b.HasIndex(x => new { x.SiteKey, x.Kind, x.Slug }).IsUnique();
        });

        modelBuilder.Entity<ContentTaxonomy>(b =>
        {
            b.ToTable("content_taxonomies");
            b.HasKey(x => new { x.ContentId, x.TaxonomyTermId });
            b.HasOne(x => x.Content).WithMany(x => x.Taxonomies).HasForeignKey(x => x.ContentId).OnDelete(DeleteBehavior.Cascade);
            b.HasOne(x => x.TaxonomyTerm).WithMany().HasForeignKey(x => x.TaxonomyTermId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MediaReference>(b =>
        {
            b.ToTable("media_references");
            b.HasKey(x => x.Id);
            b.Property(x => x.FileName).HasMaxLength(500);
            b.Property(x => x.ContentType).HasMaxLength(160);
            b.Property(x => x.AltText).HasMaxLength(600);
            b.Property(x => x.Caption).HasMaxLength(1200);
            b.Property(x => x.MetadataJson).HasColumnType("jsonb");
            b.HasIndex(x => x.StorageFileId).IsUnique();
        });

        modelBuilder.Entity<NavigationMenu>(b =>
        {
            b.ToTable("navigation_menus");
            b.HasKey(x => x.Id);
            b.Property(x => x.SiteKey).HasMaxLength(80);
            b.Property(x => x.Name).HasMaxLength(200);
            b.Property(x => x.Location).HasMaxLength(100);
            b.HasIndex(x => new { x.SiteKey, x.Location }).IsUnique();
            b.HasMany(x => x.Items).WithOne(x => x.Menu).HasForeignKey(x => x.MenuId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<NavigationMenuItem>(b =>
        {
            b.ToTable("navigation_menu_items");
            b.HasKey(x => x.Id);
            b.Property(x => x.Label).HasMaxLength(200);
            b.Property(x => x.Url).HasMaxLength(1200);
            b.Property(x => x.Target).HasMaxLength(20);
            b.HasIndex(x => new { x.MenuId, x.SortOrder });
        });

        modelBuilder.Entity<SiteSetting>(b =>
        {
            b.ToTable("site_settings");
            b.HasKey(x => x.Id);
            b.Property(x => x.SiteKey).HasMaxLength(80);
            b.Property(x => x.Key).HasMaxLength(200);
            b.Property(x => x.ValueJson).HasColumnType("jsonb");
            b.HasIndex(x => new { x.SiteKey, x.Key }).IsUnique();
        });

        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
        modelBuilder.ApplyConfiguration(new InboxMessageConfiguration());
    }
}
