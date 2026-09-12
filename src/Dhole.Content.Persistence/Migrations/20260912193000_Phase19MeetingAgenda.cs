using Dhole.Content.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dhole.Content.Persistence.Migrations;

[DbContext(typeof(ServiceDbContext))]
[Migration("20260912193000_Phase19MeetingAgenda")]
public sealed class Phase19MeetingAgenda : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
CREATE TABLE content.meeting_types (
    id uuid PRIMARY KEY,
    site_key varchar(80) NOT NULL,
    name varchar(240) NOT NULL,
    slug varchar(160) NOT NULL,
    description varchar(4000),
    duration_minutes integer NOT NULL,
    buffer_minutes integer NOT NULL,
    meeting_mode varchar(80) NOT NULL,
    assigned_user_id uuid,
    assigned_team_key varchar(160),
    settings_json jsonb,
    is_active boolean NOT NULL,
    created_at_utc timestamptz NOT NULL,
    created_by text,
    updated_at_utc timestamptz,
    updated_by text,
    is_deleted boolean NOT NULL DEFAULT false,
    deleted_at_utc timestamptz,
    deleted_by text,
    CONSTRAINT ck_meeting_types_duration CHECK (duration_minutes > 0 AND duration_minutes <= 1440),
    CONSTRAINT ck_meeting_types_buffer CHECK (buffer_minutes >= 0 AND buffer_minutes <= 1440)
);
CREATE UNIQUE INDEX ux_meeting_types_site_slug ON content.meeting_types(site_key, slug) WHERE is_deleted = false;
CREATE INDEX ix_meeting_types_site_active ON content.meeting_types(site_key, is_active) WHERE is_deleted = false;

CREATE TABLE content.meeting_requests (
    id uuid PRIMARY KEY,
    meeting_type_id uuid NOT NULL,
    lead_id uuid,
    submission_id uuid,
    requested_start_utc timestamptz NOT NULL,
    requested_end_utc timestamptz NOT NULL,
    time_zone varchar(120) NOT NULL,
    subject varchar(240) NOT NULL,
    message varchar(4000),
    status varchar(80) NOT NULL,
    assigned_user_id uuid,
    confirmed_start_utc timestamptz,
    confirmed_end_utc timestamptz,
    external_provider varchar(120),
    external_event_id varchar(240),
    meeting_url varchar(2048),
    created_at_utc timestamptz NOT NULL,
    created_by text,
    updated_at_utc timestamptz,
    updated_by text,
    is_deleted boolean NOT NULL DEFAULT false,
    deleted_at_utc timestamptz,
    deleted_by text,
    CONSTRAINT fk_meeting_requests_type FOREIGN KEY (meeting_type_id) REFERENCES content.meeting_types(id) ON DELETE RESTRICT,
    CONSTRAINT fk_meeting_requests_lead FOREIGN KEY (lead_id) REFERENCES content.marketing_leads(id) ON DELETE SET NULL,
    CONSTRAINT fk_meeting_requests_submission FOREIGN KEY (submission_id) REFERENCES content.marketing_submissions(id) ON DELETE SET NULL,
    CONSTRAINT ck_meeting_requests_identity CHECK (lead_id IS NOT NULL OR submission_id IS NOT NULL),
    CONSTRAINT ck_meeting_requests_requested_window CHECK (requested_end_utc > requested_start_utc),
    CONSTRAINT ck_meeting_requests_confirmed_window CHECK (confirmed_start_utc IS NULL OR confirmed_end_utc IS NULL OR confirmed_end_utc > confirmed_start_utc),
    CONSTRAINT ck_meeting_requests_status CHECK (status IN ('Requested','PendingConfirmation','Confirmed','Rejected','Cancelled','Completed'))
);
CREATE INDEX ix_meeting_requests_type_start ON content.meeting_requests(meeting_type_id, requested_start_utc) WHERE is_deleted = false;
CREATE INDEX ix_meeting_requests_status_start ON content.meeting_requests(status, requested_start_utc) WHERE is_deleted = false;
CREATE INDEX ix_meeting_requests_lead ON content.meeting_requests(lead_id) WHERE lead_id IS NOT NULL AND is_deleted = false;
CREATE INDEX ix_meeting_requests_submission ON content.meeting_requests(submission_id) WHERE submission_id IS NOT NULL AND is_deleted = false;
CREATE INDEX ix_meeting_requests_assigned_user ON content.meeting_requests(assigned_user_id) WHERE assigned_user_id IS NOT NULL AND is_deleted = false;
""");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
        => migrationBuilder.Sql("DROP TABLE IF EXISTS content.meeting_requests; DROP TABLE IF EXISTS content.meeting_types;");
}
