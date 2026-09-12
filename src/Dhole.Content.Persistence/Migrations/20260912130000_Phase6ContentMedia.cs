using Dhole.Content.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dhole.Content.Persistence.Migrations;

[DbContext(typeof(ServiceDbContext))]
[Migration("20260912130000_Phase6ContentMedia")]
public sealed class Phase6ContentMedia : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
CREATE TABLE content.content_media (
    id uuid PRIMARY KEY,
    content_id uuid NOT NULL,
    media_reference_id uuid NOT NULL,
    role varchar(80) NOT NULL,
    sort_order integer NOT NULL DEFAULT 0,
    alt_text_override varchar(600),
    caption_override varchar(1200),
    focal_x numeric(5,4),
    focal_y numeric(5,4),
    settings_json jsonb,
    created_at_utc timestamptz NOT NULL,
    created_by text,
    updated_at_utc timestamptz,
    updated_by text,
    is_deleted boolean NOT NULL DEFAULT false,
    deleted_at_utc timestamptz,
    deleted_by text,
    CONSTRAINT fk_content_media_content FOREIGN KEY (content_id)
        REFERENCES content.content_items(id) ON DELETE CASCADE,
    CONSTRAINT fk_content_media_media_reference FOREIGN KEY (media_reference_id)
        REFERENCES content.media_references(id) ON DELETE CASCADE,
    CONSTRAINT ck_content_media_sort_order CHECK (sort_order >= 0),
    CONSTRAINT ck_content_media_focal_x CHECK (focal_x IS NULL OR (focal_x >= 0 AND focal_x <= 1)),
    CONSTRAINT ck_content_media_focal_y CHECK (focal_y IS NULL OR (focal_y >= 0 AND focal_y <= 1)),
    CONSTRAINT ck_content_media_role CHECK (role IN (
        'hero',
        'featured',
        'gallery',
        'thumbnail',
        'background',
        'video',
        'video.poster',
        'banner.desktop',
        'banner.mobile',
        'open-graph'
    ))
);

CREATE UNIQUE INDEX ux_content_media_content_media_role
    ON content.content_media(content_id, media_reference_id, role)
    WHERE is_deleted = false;

CREATE INDEX ix_content_media_content_role_sort
    ON content.content_media(content_id, role, sort_order)
    WHERE is_deleted = false;
""");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
        => migrationBuilder.Sql("DROP TABLE IF EXISTS content.content_media;");
}
