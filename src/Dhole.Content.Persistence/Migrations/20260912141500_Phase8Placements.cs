using Dhole.Content.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dhole.Content.Persistence.Migrations;

[DbContext(typeof(ServiceDbContext))]
[Migration("20260912141500_Phase8Placements")]
public sealed class Phase8Placements : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
CREATE TABLE content.placements (
    id uuid PRIMARY KEY,
    site_key varchar(80) NOT NULL,
    code varchar(160) NOT NULL,
    name varchar(240) NOT NULL,
    allowed_types_json jsonb NOT NULL,
    max_items integer NOT NULL,
    settings_json jsonb,
    is_active boolean NOT NULL DEFAULT true,
    created_at_utc timestamptz NOT NULL,
    created_by text,
    updated_at_utc timestamptz,
    updated_by text,
    is_deleted boolean NOT NULL DEFAULT false,
    deleted_at_utc timestamptz,
    deleted_by text,
    CONSTRAINT ck_placements_max_items CHECK (max_items > 0)
);

CREATE UNIQUE INDEX ux_placements_site_code
    ON content.placements(site_key, code)
    WHERE is_deleted = false;

CREATE TABLE content.placement_items (
    id uuid PRIMARY KEY,
    placement_id uuid NOT NULL,
    content_id uuid NOT NULL,
    sort_order integer NOT NULL DEFAULT 0,
    valid_from_utc timestamptz,
    valid_to_utc timestamptz,
    settings_json jsonb,
    is_active boolean NOT NULL DEFAULT true,
    created_at_utc timestamptz NOT NULL,
    created_by text,
    updated_at_utc timestamptz,
    updated_by text,
    is_deleted boolean NOT NULL DEFAULT false,
    deleted_at_utc timestamptz,
    deleted_by text,
    CONSTRAINT fk_placement_items_placement FOREIGN KEY (placement_id)
        REFERENCES content.placements(id) ON DELETE CASCADE,
    CONSTRAINT fk_placement_items_content FOREIGN KEY (content_id)
        REFERENCES content.content_items(id) ON DELETE CASCADE,
    CONSTRAINT ck_placement_items_sort_order CHECK (sort_order >= 0),
    CONSTRAINT ck_placement_items_validity CHECK (valid_to_utc IS NULL OR valid_from_utc IS NULL OR valid_to_utc > valid_from_utc)
);

CREATE UNIQUE INDEX ux_placement_items_placement_content
    ON content.placement_items(placement_id, content_id)
    WHERE is_deleted = false;

CREATE INDEX ix_placement_items_placement_sort
    ON content.placement_items(placement_id, sort_order)
    WHERE is_deleted = false;
""");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DROP TABLE IF EXISTS content.placement_items;");
        migrationBuilder.Sql("DROP TABLE IF EXISTS content.placements;");
    }
}
