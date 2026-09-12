using Dhole.Content.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dhole.Content.Persistence.Migrations;

[DbContext(typeof(ServiceDbContext))]
[Migration("20260912112000_Phase2ContentItemLocalization")]
public sealed class Phase2ContentItemLocalization : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
ALTER TABLE content.content_items
    ADD COLUMN parent_content_id uuid,
    ADD COLUMN translation_group_id uuid,
    ADD COLUMN template_key varchar(120),
    ADD COLUMN unpublish_at_utc timestamptz,
    ADD COLUMN sitemap_priority numeric(3,2),
    ADD COLUMN sitemap_change_frequency varchar(30);

ALTER TABLE content.content_items
    ADD CONSTRAINT ck_content_items_sitemap_priority
    CHECK (sitemap_priority IS NULL OR (sitemap_priority >= 0 AND sitemap_priority <= 1));

ALTER TABLE content.content_items
    ADD CONSTRAINT fk_content_items_parent_content
    FOREIGN KEY (parent_content_id)
    REFERENCES content.content_items(id)
    ON DELETE SET NULL;

DROP INDEX IF EXISTS content.ux_content_items_site_slug;

CREATE UNIQUE INDEX ux_content_items_site_locale_slug
    ON content.content_items(site_key, locale, slug)
    WHERE is_deleted = false;

CREATE INDEX ix_content_items_parent_content_id
    ON content.content_items(parent_content_id);

CREATE INDEX ix_content_items_translation_group
    ON content.content_items(site_key, translation_group_id)
    WHERE translation_group_id IS NOT NULL AND is_deleted = false;
""");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
ALTER TABLE content.content_items
    DROP CONSTRAINT IF EXISTS fk_content_items_parent_content;

ALTER TABLE content.content_items
    DROP CONSTRAINT IF EXISTS ck_content_items_sitemap_priority;

DROP INDEX IF EXISTS content.ix_content_items_translation_group;
DROP INDEX IF EXISTS content.ix_content_items_parent_content_id;
DROP INDEX IF EXISTS content.ux_content_items_site_locale_slug;

CREATE UNIQUE INDEX ux_content_items_site_slug
    ON content.content_items(site_key, slug)
    WHERE is_deleted = false;

ALTER TABLE content.content_items
    DROP COLUMN IF EXISTS sitemap_change_frequency,
    DROP COLUMN IF EXISTS sitemap_priority,
    DROP COLUMN IF EXISTS unpublish_at_utc,
    DROP COLUMN IF EXISTS template_key,
    DROP COLUMN IF EXISTS translation_group_id,
    DROP COLUMN IF EXISTS parent_content_id;
""");
    }
}
