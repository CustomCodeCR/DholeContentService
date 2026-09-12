using Dhole.Content.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dhole.Content.Persistence.Migrations;

[DbContext(typeof(ServiceDbContext))]
[Migration("20260912210500_Phase21Campaigns")]
public sealed class Phase21Campaigns : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
CREATE TABLE content.campaigns (
    id uuid PRIMARY KEY,
    site_key varchar(80) NOT NULL,
    name varchar(240) NOT NULL,
    slug varchar(160) NOT NULL,
    status varchar(80) NOT NULL,
    starts_at_utc timestamptz,
    ends_at_utc timestamptz,
    landing_content_id uuid,
    utm_source varchar(240),
    utm_medium varchar(240),
    utm_campaign varchar(240),
    goal_type varchar(120) NOT NULL,
    settings_json jsonb,
    created_at_utc timestamptz NOT NULL,
    created_by text,
    updated_at_utc timestamptz,
    updated_by text,
    is_deleted boolean NOT NULL DEFAULT false,
    deleted_at_utc timestamptz,
    deleted_by text,
    CONSTRAINT fk_campaigns_landing_content FOREIGN KEY (landing_content_id) REFERENCES content.content_items(id) ON DELETE SET NULL,
    CONSTRAINT ck_campaigns_window CHECK (starts_at_utc IS NULL OR ends_at_utc IS NULL OR ends_at_utc > starts_at_utc)
);
CREATE UNIQUE INDEX ux_campaigns_site_slug ON content.campaigns(site_key, slug) WHERE is_deleted = false;
CREATE INDEX ix_campaigns_site_status ON content.campaigns(site_key, status) WHERE is_deleted = false;
CREATE INDEX ix_campaigns_window ON content.campaigns(starts_at_utc, ends_at_utc) WHERE is_deleted = false;

ALTER TABLE content.marketing_submissions
    ADD CONSTRAINT fk_marketing_submissions_campaign
    FOREIGN KEY (campaign_id) REFERENCES content.campaigns(id) ON DELETE SET NULL NOT VALID;
""");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
        => migrationBuilder.Sql("""
ALTER TABLE content.marketing_submissions DROP CONSTRAINT IF EXISTS fk_marketing_submissions_campaign;
DROP TABLE IF EXISTS content.campaigns;
""");
}
