using Dhole.Content.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dhole.Content.Persistence.Migrations;

[DbContext(typeof(ServiceDbContext))]
[Migration("20260912183000_Phase17MarketingLeads")]
public sealed class Phase17MarketingLeads : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
CREATE TABLE content.marketing_leads (
    id uuid PRIMARY KEY,
    site_key varchar(80) NOT NULL,
    first_name varchar(160),
    last_name varchar(160),
    email varchar(320),
    phone varchar(60),
    company varchar(240),
    job_title varchar(160),
    country varchar(120),
    source varchar(160),
    status varchar(80) NOT NULL,
    owner_user_id uuid,
    first_touch_at_utc timestamptz NOT NULL,
    last_touch_at_utc timestamptz NOT NULL,
    created_at_utc timestamptz NOT NULL,
    created_by text,
    updated_at_utc timestamptz,
    updated_by text,
    is_deleted boolean NOT NULL DEFAULT false,
    deleted_at_utc timestamptz,
    deleted_by text,
    CONSTRAINT ck_marketing_leads_contact CHECK (email IS NOT NULL OR phone IS NOT NULL),
    CONSTRAINT ck_marketing_leads_touch_window CHECK (last_touch_at_utc >= first_touch_at_utc)
);

CREATE UNIQUE INDEX ux_marketing_leads_site_email
    ON content.marketing_leads(site_key, email)
    WHERE email IS NOT NULL AND is_deleted = false;

CREATE INDEX ix_marketing_leads_site_status
    ON content.marketing_leads(site_key, status)
    WHERE is_deleted = false;

CREATE INDEX ix_marketing_leads_owner
    ON content.marketing_leads(owner_user_id)
    WHERE owner_user_id IS NOT NULL AND is_deleted = false;

CREATE INDEX ix_marketing_leads_last_touch
    ON content.marketing_leads(last_touch_at_utc)
    WHERE is_deleted = false;
""");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
        => migrationBuilder.Sql("DROP TABLE IF EXISTS content.marketing_leads;");
}
