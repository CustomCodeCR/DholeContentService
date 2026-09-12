using Dhole.Content.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dhole.Content.Persistence.Migrations;

[DbContext(typeof(ServiceDbContext))]
[Migration("20260912190000_Phase18MarketingConsents")]
public sealed class Phase18MarketingConsents : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
CREATE TABLE content.marketing_consents (
    id uuid PRIMARY KEY,
    lead_id uuid,
    submission_id uuid,
    purpose varchar(80) NOT NULL,
    granted boolean NOT NULL,
    policy_version varchar(80) NOT NULL,
    source varchar(160) NOT NULL,
    captured_at_utc timestamptz NOT NULL,
    created_at_utc timestamptz NOT NULL,
    created_by text,
    updated_at_utc timestamptz,
    updated_by text,
    is_deleted boolean NOT NULL DEFAULT false,
    deleted_at_utc timestamptz,
    deleted_by text,
    CONSTRAINT ck_marketing_consents_reference CHECK (lead_id IS NOT NULL OR submission_id IS NOT NULL),
    CONSTRAINT ck_marketing_consents_purpose CHECK (purpose IN ('privacy','contact','newsletter','commercial-communications')),
    CONSTRAINT ck_marketing_consents_capture_time CHECK (captured_at_utc <= created_at_utc),
    CONSTRAINT fk_marketing_consents_lead FOREIGN KEY (lead_id)
        REFERENCES content.marketing_leads(id) ON DELETE RESTRICT,
    CONSTRAINT fk_marketing_consents_submission FOREIGN KEY (submission_id)
        REFERENCES content.marketing_submissions(id) ON DELETE RESTRICT
);

CREATE INDEX ix_marketing_consents_lead_purpose_captured
    ON content.marketing_consents(lead_id, purpose, captured_at_utc)
    WHERE lead_id IS NOT NULL AND is_deleted = false;

CREATE INDEX ix_marketing_consents_submission_purpose_captured
    ON content.marketing_consents(submission_id, purpose, captured_at_utc)
    WHERE submission_id IS NOT NULL AND is_deleted = false;

CREATE INDEX ix_marketing_consents_purpose_captured
    ON content.marketing_consents(purpose, captured_at_utc)
    WHERE is_deleted = false;
""");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
        => migrationBuilder.Sql("DROP TABLE IF EXISTS content.marketing_consents;");
}
