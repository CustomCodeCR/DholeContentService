using Dhole.Content.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dhole.Content.Persistence.Migrations;

[DbContext(typeof(ServiceDbContext))]
[Migration("20260912114500_Phase3Sites")]
public sealed class Phase3Sites : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
CREATE TABLE content.sites (
    id uuid PRIMARY KEY,
    site_key varchar(80) NOT NULL,
    name varchar(200) NOT NULL,
    primary_domain varchar(255) NOT NULL,
    default_locale varchar(20) NOT NULL,
    time_zone varchar(100) NOT NULL,
    logo_media_id uuid,
    favicon_media_id uuid,
    default_open_graph_media_id uuid,
    status varchar(40) NOT NULL,
    created_at_utc timestamptz NOT NULL,
    created_by text,
    updated_at_utc timestamptz,
    updated_by text,
    is_deleted boolean NOT NULL DEFAULT false,
    deleted_at_utc timestamptz,
    deleted_by text,
    CONSTRAINT fk_sites_logo_media FOREIGN KEY (logo_media_id)
        REFERENCES content.media_references(id) ON DELETE SET NULL,
    CONSTRAINT fk_sites_favicon_media FOREIGN KEY (favicon_media_id)
        REFERENCES content.media_references(id) ON DELETE SET NULL,
    CONSTRAINT fk_sites_default_og_media FOREIGN KEY (default_open_graph_media_id)
        REFERENCES content.media_references(id) ON DELETE SET NULL
);

CREATE UNIQUE INDEX ux_sites_site_key
    ON content.sites(site_key)
    WHERE is_deleted = false;

CREATE UNIQUE INDEX ux_sites_primary_domain
    ON content.sites(primary_domain)
    WHERE is_deleted = false;

INSERT INTO content.sites (
    id,
    site_key,
    name,
    primary_domain,
    default_locale,
    time_zone,
    status,
    created_at_utc,
    is_deleted
)
VALUES (
    '11111111-1111-1111-1111-111111111111',
    'main',
    'Logística Castro Fallas',
    'logisticacastrofallas.com',
    'es-CR',
    'America/Costa_Rica',
    'Active',
    CURRENT_TIMESTAMP,
    false
)
ON CONFLICT DO NOTHING;
""");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
        => migrationBuilder.Sql("DROP TABLE IF EXISTS content.sites;");
}
