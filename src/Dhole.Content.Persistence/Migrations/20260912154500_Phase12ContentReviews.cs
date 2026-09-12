using Dhole.Content.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dhole.Content.Persistence.Migrations;

[DbContext(typeof(ServiceDbContext))]
[Migration("20260912154500_Phase12ContentReviews")]
public sealed class Phase12ContentReviews : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
CREATE TABLE content.content_reviews (
    id uuid PRIMARY KEY,
    content_id uuid NOT NULL,
    revision_id uuid NOT NULL,
    submitted_by_user_id uuid,
    reviewer_user_id uuid,
    status varchar(30) NOT NULL,
    comment varchar(2000),
    submitted_at_utc timestamptz NOT NULL,
    decided_at_utc timestamptz,
    created_at_utc timestamptz NOT NULL,
    created_by text,
    updated_at_utc timestamptz,
    updated_by text,
    is_deleted boolean NOT NULL DEFAULT false,
    deleted_at_utc timestamptz,
    deleted_by text,
    CONSTRAINT fk_content_reviews_content FOREIGN KEY (content_id)
        REFERENCES content.content_items(id) ON DELETE CASCADE,
    CONSTRAINT fk_content_reviews_revision FOREIGN KEY (revision_id)
        REFERENCES content.content_revisions(id) ON DELETE CASCADE,
    CONSTRAINT ck_content_reviews_status CHECK (status IN ('Pending', 'Approved', 'Rejected')),
    CONSTRAINT ck_content_reviews_decision CHECK (
        (status = 'Pending' AND decided_at_utc IS NULL)
        OR (status IN ('Approved', 'Rejected') AND decided_at_utc IS NOT NULL)
    )
);

CREATE INDEX ix_content_reviews_content_submitted
    ON content.content_reviews(content_id, submitted_at_utc DESC)
    WHERE is_deleted = false;

CREATE UNIQUE INDEX ux_content_reviews_pending_content
    ON content.content_reviews(content_id)
    WHERE is_deleted = false AND status = 'Pending';
""");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
        => migrationBuilder.Sql("DROP TABLE IF EXISTS content.content_reviews;");
}
