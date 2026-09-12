using Dhole.Content.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dhole.Content.Persistence.Migrations;

[DbContext(typeof(ServiceDbContext))]
[Migration("20260912170000_Phase15MarketingForms")]
public sealed class Phase15MarketingForms : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
CREATE TABLE content.marketing_forms (
    id uuid PRIMARY KEY,
    site_key varchar(80) NOT NULL,
    form_key varchar(160) NOT NULL,
    name varchar(240) NOT NULL,
    purpose varchar(80) NOT NULL,
    status varchar(40) NOT NULL DEFAULT 'Draft',
    success_message varchar(1000),
    notification_template_key varchar(160),
    settings_json jsonb,
    version integer NOT NULL DEFAULT 1,
    created_at_utc timestamptz NOT NULL,
    created_by text,
    updated_at_utc timestamptz,
    updated_by text,
    is_deleted boolean NOT NULL DEFAULT false,
    deleted_at_utc timestamptz,
    deleted_by text,
    CONSTRAINT ck_marketing_forms_settings_json CHECK (settings_json IS NULL OR jsonb_typeof(settings_json) = 'object'),
    CONSTRAINT ck_marketing_forms_version CHECK (version >= 1),
    CONSTRAINT ck_marketing_forms_purpose CHECK (purpose IN ('contact', 'quote-request', 'meeting', 'newsletter', 'campaign')),
    CONSTRAINT ck_marketing_forms_status CHECK (status IN ('Draft', 'Active', 'Inactive'))
);

CREATE UNIQUE INDEX ux_marketing_forms_site_form_key
    ON content.marketing_forms(site_key, form_key)
    WHERE is_deleted = false;

CREATE INDEX ix_marketing_forms_site_purpose_status
    ON content.marketing_forms(site_key, purpose, status)
    WHERE is_deleted = false;

CREATE TABLE content.marketing_form_fields (
    id uuid PRIMARY KEY,
    form_id uuid NOT NULL,
    field_key varchar(160) NOT NULL,
    label varchar(240) NOT NULL,
    field_type varchar(80) NOT NULL,
    placeholder varchar(500),
    is_required boolean NOT NULL DEFAULT false,
    sort_order integer NOT NULL DEFAULT 0,
    validation_json jsonb,
    options_json jsonb,
    created_at_utc timestamptz NOT NULL,
    created_by text,
    updated_at_utc timestamptz,
    updated_by text,
    is_deleted boolean NOT NULL DEFAULT false,
    deleted_at_utc timestamptz,
    deleted_by text,
    CONSTRAINT fk_marketing_form_fields_form FOREIGN KEY (form_id)
        REFERENCES content.marketing_forms(id) ON DELETE CASCADE,
    CONSTRAINT ck_marketing_form_fields_sort_order CHECK (sort_order >= 0),
    CONSTRAINT ck_marketing_form_fields_validation_json CHECK (validation_json IS NULL OR jsonb_typeof(validation_json) = 'object'),
    CONSTRAINT ck_marketing_form_fields_options_json CHECK (options_json IS NULL OR jsonb_typeof(options_json) = 'array')
);

CREATE UNIQUE INDEX ux_marketing_form_fields_form_field_key
    ON content.marketing_form_fields(form_id, field_key)
    WHERE is_deleted = false;

CREATE INDEX ix_marketing_form_fields_form_sort
    ON content.marketing_form_fields(form_id, sort_order)
    WHERE is_deleted = false;
""");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
        => migrationBuilder.Sql("""
DROP TABLE IF EXISTS content.marketing_form_fields;
DROP TABLE IF EXISTS content.marketing_forms;
""");
}
