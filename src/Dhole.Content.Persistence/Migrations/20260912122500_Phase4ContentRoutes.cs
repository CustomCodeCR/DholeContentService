using Dhole.Content.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dhole.Content.Persistence.Migrations;

[DbContext(typeof(ServiceDbContext))]
[Migration("20260912122500_Phase4ContentRoutes")]
public sealed class Phase4ContentRoutes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
CREATE TABLE content.content_routes (
    id uuid PRIMARY KEY,
    site_key varchar(80) NOT NULL,
    content_id uuid NOT NULL,
    locale varchar(20) NOT NULL,
    path varchar(1200) NOT NULL,
    is_primary boolean NOT NULL DEFAULT false,
    is_active boolean NOT NULL DEFAULT true,
    created_at_utc timestamptz NOT NULL,
    created_by text,
    updated_at_utc timestamptz,
    updated_by text,
    CONSTRAINT fk_content_routes_content FOREIGN KEY (content_id)
        REFERENCES content.content_items(id) ON DELETE CASCADE
);

CREATE UNIQUE INDEX ux_content_routes_site_locale_path
    ON content.content_routes(site_key, locale, path);

CREATE UNIQUE INDEX ux_content_routes_primary
    ON content.content_routes(content_id, locale)
    WHERE is_primary = true;

CREATE INDEX ix_content_routes_content
    ON content.content_routes(site_key, content_id, locale);

-- Bootstrap a primary route for content that already exists before FASE 4.
-- Existing slug endpoints remain as a compatibility layer, while new consumers
-- resolve canonical URLs from content.content_routes.
INSERT INTO content.content_routes (
    id,
    site_key,
    content_id,
    locale,
    path,
    is_primary,
    is_active,
    created_at_utc,
    created_by
)
SELECT
    ci.id,
    lower(ci.site_key),
    ci.id,
    ci.locale,
    '/' || lower(split_part(ci.locale, '-', 1)) || '/' || trim(both '/' from ci.slug),
    true,
    true,
    COALESCE(ci.created_at_utc, CURRENT_TIMESTAMP),
    ci.created_by
FROM content.content_items ci
WHERE ci.is_deleted = false
ON CONFLICT DO NOTHING;
""");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
        => migrationBuilder.Sql("DROP TABLE IF EXISTS content.content_routes;");
}
