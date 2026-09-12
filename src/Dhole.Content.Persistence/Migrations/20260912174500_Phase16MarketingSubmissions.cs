using Dhole.Content.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dhole.Content.Persistence.Migrations;

[DbContext(typeof(ServiceDbContext))]
[Migration("20260912174500_Phase16MarketingSubmissions")]
public sealed class Phase16MarketingSubmissions : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
CREATE TABLE content.marketing_submissions (
    id uuid PRIMARY KEY,
    form_id uuid NOT NULL,
    content_id uuid,
    campaign_id uuid,
    submitted_at_utc timestamptz NOT NULL,
    status varchar(80) NOT NULL DEFAULT 'Received',
    source_url varchar(2048) NOT NULL,
    referrer_url varchar(2048),
    utm_source varchar(240),
    utm_medium varchar(240),
    utm_campaign varchar(240),
    utm_content varchar(240),
    utm_term varchar(240),
    payload_json jsonb NOT NULL,
    ip_hash varchar(64),
    user_agent varchar(1024),
    correlation_id uuid NOT NULL,
    created_at_utc timestamptz NOT NULL,
    created_by text,
    updated_at_utc timestamptz,
    updated_by text,
    is_deleted boolean NOT NULL DEFAULT false,
    deleted_at_utc timestamptz,
    deleted_by text,
    CONSTRAINT fk_marketing_submissions_form FOREIGN KEY (form_id)
        REFERENCES content.marketing_forms(id) ON DELETE RESTRICT,
    CONSTRAINT fk_marketing_submissions_content FOREIGN KEY (content_id)
        REFERENCES content.content_items(id) ON DELETE SET NULL,
    CONSTRAINT ck_marketing_submissions_payload_json CHECK (jsonb_typeof(payload_json) = 'object'),
    CONSTRAINT ck_marketing_submissions_ip_hash CHECK (ip_hash IS NULL OR length(ip_hash) = 64)
);

CREATE INDEX ix_marketing_submissions_form_submitted
    ON content.marketing_submissions(form_id, submitted_at_utc)
    WHERE is_deleted = false;

CREATE UNIQUE INDEX ux_marketing_submissions_correlation
    ON content.marketing_submissions(correlation_id)
    WHERE is_deleted = false;

CREATE INDEX ix_marketing_submissions_campaign
    ON content.marketing_submissions(campaign_id)
    WHERE campaign_id IS NOT NULL AND is_deleted = false;
""");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
        => migrationBuilder.Sql("DROP TABLE IF EXISTS content.marketing_submissions;");
}
