using Dhole.Content.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dhole.Content.Persistence.Migrations;

[DbContext(typeof(ServiceDbContext))]
[Migration("20260912150000_Phase11Redirects")]
public sealed class Phase11Redirects : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
CREATE TABLE content.redirects (
    id uuid PRIMARY KEY,
    site_key varchar(80) NOT NULL,
    source_path varchar(1000) NOT NULL,
    target_url varchar(2000) NOT NULL,
    status_code integer NOT NULL,
    is_active boolean NOT NULL DEFAULT true,
    valid_from_utc timestamptz,
    valid_to_utc timestamptz,
    created_at_utc timestamptz NOT NULL,
    created_by text,
    updated_at_utc timestamptz,
    updated_by text,
    is_deleted boolean NOT NULL DEFAULT false,
    deleted_at_utc timestamptz,
    deleted_by text,
    CONSTRAINT ck_redirects_status_code CHECK (status_code IN (301, 302)),
    CONSTRAINT ck_redirects_validity CHECK (valid_to_utc IS NULL OR valid_from_utc IS NULL OR valid_to_utc > valid_from_utc)
);

CREATE UNIQUE INDEX ux_redirects_site_source_path
    ON content.redirects(site_key, source_path)
    WHERE is_deleted = false;

CREATE INDEX ix_redirects_resolution
    ON content.redirects(site_key, is_active, valid_from_utc, valid_to_utc)
    WHERE is_deleted = false;
""");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
        => migrationBuilder.Sql("DROP TABLE IF EXISTS content.redirects;");
}
