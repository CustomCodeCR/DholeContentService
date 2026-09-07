using Dhole.Content.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dhole.Content.Persistence.Migrations;

[DbContext(typeof(ServiceDbContext))]
[Migration("20260907190000_InitialContentCms")]
public sealed class InitialContentCms : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
CREATE SCHEMA IF NOT EXISTS content;

CREATE TABLE content.content_items (
    id uuid PRIMARY KEY,
    site_key varchar(80) NOT NULL,
    type varchar(40) NOT NULL,
    status varchar(40) NOT NULL,
    title varchar(300) NOT NULL,
    slug varchar(300) NOT NULL,
    excerpt varchar(1200),
    blocks_json jsonb NOT NULL DEFAULT '[]'::jsonb,
    rendered_html text,
    featured_media_id uuid,
    author_user_id uuid,
    locale varchar(20) NOT NULL DEFAULT 'es-CR',
    sort_order integer NOT NULL DEFAULT 0,
    is_featured boolean NOT NULL DEFAULT false,
    seo_title varchar(300),
    seo_description varchar(600),
    seo_keywords varchar(1000),
    canonical_url varchar(1000),
    robots varchar(120),
    open_graph_media_id uuid,
    structured_data_json jsonb,
    scheduled_at_utc timestamptz,
    published_at_utc timestamptz,
    created_at_utc timestamptz NOT NULL,
    created_by text,
    updated_at_utc timestamptz,
    updated_by text,
    is_deleted boolean NOT NULL DEFAULT false,
    deleted_at_utc timestamptz,
    deleted_by text
);
CREATE UNIQUE INDEX ux_content_items_site_slug ON content.content_items(site_key, slug) WHERE is_deleted = false;
CREATE INDEX ix_content_items_public ON content.content_items(site_key, type, status, published_at_utc DESC);

CREATE TABLE content.content_revisions (
    id uuid PRIMARY KEY,
    content_id uuid NOT NULL REFERENCES content.content_items(id) ON DELETE CASCADE,
    revision_number integer NOT NULL,
    title varchar(300) NOT NULL,
    slug varchar(300) NOT NULL,
    excerpt varchar(1200),
    blocks_json jsonb NOT NULL DEFAULT '[]'::jsonb,
    rendered_html text,
    featured_media_id uuid,
    seo_title varchar(300),
    seo_description varchar(600),
    seo_keywords varchar(1000),
    canonical_url varchar(1000),
    robots varchar(120),
    open_graph_media_id uuid,
    structured_data_json jsonb,
    reason varchar(300),
    created_by uuid,
    created_at_utc timestamptz NOT NULL
);
CREATE UNIQUE INDEX ux_content_revisions_number ON content.content_revisions(content_id, revision_number);

CREATE TABLE content.taxonomy_terms (
    id uuid PRIMARY KEY,
    site_key varchar(80) NOT NULL,
    kind varchar(30) NOT NULL,
    name varchar(200) NOT NULL,
    slug varchar(200) NOT NULL,
    description varchar(1200),
    parent_id uuid,
    sort_order integer NOT NULL DEFAULT 0,
    created_at_utc timestamptz NOT NULL,
    created_by text,
    updated_at_utc timestamptz,
    updated_by text,
    is_deleted boolean NOT NULL DEFAULT false,
    deleted_at_utc timestamptz,
    deleted_by text
);
CREATE UNIQUE INDEX ux_taxonomy_terms_slug ON content.taxonomy_terms(site_key, kind, slug) WHERE is_deleted = false;
CREATE INDEX ix_taxonomy_terms_order ON content.taxonomy_terms(site_key, kind, sort_order);

CREATE TABLE content.content_taxonomies (
    content_id uuid NOT NULL REFERENCES content.content_items(id) ON DELETE CASCADE,
    taxonomy_term_id uuid NOT NULL REFERENCES content.taxonomy_terms(id) ON DELETE CASCADE,
    PRIMARY KEY(content_id, taxonomy_term_id)
);

CREATE TABLE content.media_references (
    id uuid PRIMARY KEY,
    storage_file_id uuid NOT NULL,
    file_name varchar(500) NOT NULL,
    content_type varchar(160) NOT NULL,
    alt_text varchar(600),
    caption varchar(1200),
    metadata_json jsonb,
    created_at_utc timestamptz NOT NULL,
    created_by text,
    updated_at_utc timestamptz,
    updated_by text,
    is_deleted boolean NOT NULL DEFAULT false,
    deleted_at_utc timestamptz,
    deleted_by text
);
CREATE UNIQUE INDEX ux_media_references_storage ON content.media_references(storage_file_id) WHERE is_deleted = false;

CREATE TABLE content.navigation_menus (
    id uuid PRIMARY KEY,
    site_key varchar(80) NOT NULL,
    name varchar(200) NOT NULL,
    location varchar(100) NOT NULL,
    is_active boolean NOT NULL DEFAULT true,
    created_at_utc timestamptz NOT NULL,
    created_by text,
    updated_at_utc timestamptz,
    updated_by text,
    is_deleted boolean NOT NULL DEFAULT false,
    deleted_at_utc timestamptz,
    deleted_by text
);
CREATE UNIQUE INDEX ux_navigation_menus_location ON content.navigation_menus(site_key, location) WHERE is_deleted = false;

CREATE TABLE content.navigation_menu_items (
    id uuid PRIMARY KEY,
    menu_id uuid NOT NULL REFERENCES content.navigation_menus(id) ON DELETE CASCADE,
    parent_id uuid,
    label varchar(200) NOT NULL,
    url varchar(1200),
    content_id uuid,
    target varchar(20) NOT NULL DEFAULT '_self',
    sort_order integer NOT NULL DEFAULT 0,
    is_visible boolean NOT NULL DEFAULT true
);
CREATE INDEX ix_navigation_menu_items_order ON content.navigation_menu_items(menu_id, sort_order);

CREATE TABLE content.site_settings (
    id uuid PRIMARY KEY,
    site_key varchar(80) NOT NULL,
    key varchar(200) NOT NULL,
    value_json jsonb NOT NULL,
    is_public boolean NOT NULL DEFAULT false,
    created_at_utc timestamptz NOT NULL,
    created_by text,
    updated_at_utc timestamptz,
    updated_by text,
    is_deleted boolean NOT NULL DEFAULT false,
    deleted_at_utc timestamptz,
    deleted_by text
);
CREATE UNIQUE INDEX ux_site_settings_key ON content.site_settings(site_key, key) WHERE is_deleted = false;

CREATE TABLE content.inbox_messages (
    id uuid PRIMARY KEY,
    event_id uuid NOT NULL,
    event_type varchar(500) NOT NULL,
    event_name varchar(500) NOT NULL,
    source_service varchar(200) NOT NULL,
    consumer_service varchar(200) NOT NULL,
    correlation_id varchar(100),
    status varchar(50) NOT NULL,
    processed_at timestamptz,
    created_at timestamptz NOT NULL
);
CREATE UNIQUE INDEX ux_content_inbox_event_consumer ON content.inbox_messages(event_id, consumer_service);
CREATE INDEX ix_content_inbox_status_created ON content.inbox_messages(status, created_at);

CREATE TABLE content.outbox_messages (
    id uuid PRIMARY KEY,
    event_id uuid NOT NULL,
    event_type varchar(500) NOT NULL,
    event_name varchar(500) NOT NULL,
    source_service varchar(200) NOT NULL,
    payload_json jsonb NOT NULL,
    headers_json jsonb,
    correlation_id varchar(100),
    status varchar(50) NOT NULL,
    retry_count integer NOT NULL DEFAULT 0,
    error_message text,
    created_at timestamptz NOT NULL,
    processed_at timestamptz
);
CREATE UNIQUE INDEX ux_content_outbox_event ON content.outbox_messages(event_id);
CREATE INDEX ix_content_outbox_status_created ON content.outbox_messages(status, created_at);
""");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
        => migrationBuilder.Sql("DROP SCHEMA IF EXISTS content CASCADE;");
}
