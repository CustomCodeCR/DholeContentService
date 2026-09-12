using Dhole.Content.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dhole.Content.Persistence.Migrations;

[DbContext(typeof(ServiceDbContext))]
[Migration("20260912143000_Phase9Collections")]
public sealed class Phase9Collections : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
CREATE TABLE content.collections (
    id uuid PRIMARY KEY,
    site_key varchar(80) NOT NULL,
    code varchar(160) NOT NULL,
    name varchar(240) NOT NULL,
    settings_json jsonb,
    is_active boolean NOT NULL DEFAULT true,
    created_at_utc timestamptz NOT NULL,
    created_by text,
    updated_at_utc timestamptz,
    updated_by text,
    is_deleted boolean NOT NULL DEFAULT false,
    deleted_at_utc timestamptz,
    deleted_by text,
    CONSTRAINT ck_collections_settings_json CHECK (settings_json IS NULL OR jsonb_typeof(settings_json) = 'object')
);

CREATE UNIQUE INDEX ux_collections_site_code
    ON content.collections(site_key, code)
    WHERE is_deleted = false;

CREATE TABLE content.collection_items (
    id uuid PRIMARY KEY,
    collection_id uuid NOT NULL,
    data_json jsonb NOT NULL,
    sort_order integer NOT NULL DEFAULT 0,
    is_active boolean NOT NULL DEFAULT true,
    created_at_utc timestamptz NOT NULL,
    created_by text,
    updated_at_utc timestamptz,
    updated_by text,
    is_deleted boolean NOT NULL DEFAULT false,
    deleted_at_utc timestamptz,
    deleted_by text,
    CONSTRAINT fk_collection_items_collection FOREIGN KEY (collection_id)
        REFERENCES content.collections(id) ON DELETE CASCADE,
    CONSTRAINT ck_collection_items_data_json CHECK (jsonb_typeof(data_json) = 'object'),
    CONSTRAINT ck_collection_items_sort_order CHECK (sort_order >= 0)
);

CREATE INDEX ix_collection_items_collection_sort
    ON content.collection_items(collection_id, sort_order)
    WHERE is_deleted = false;
""");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
        => migrationBuilder.Sql("""
DROP TABLE IF EXISTS content.collection_items;
DROP TABLE IF EXISTS content.collections;
""");
}
