using Dhole.Content.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dhole.Content.Persistence.Migrations;

[DbContext(typeof(ServiceDbContext))]
[Migration("20260912161500_Phase13ScheduledPublication")]
public sealed class Phase13ScheduledPublication : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
CREATE INDEX IF NOT EXISTS ix_content_items_scheduled_due
    ON content.content_items(scheduled_at_utc)
    WHERE is_deleted = false AND status = 'Scheduled' AND scheduled_at_utc IS NOT NULL;

CREATE INDEX IF NOT EXISTS ix_content_items_unpublish_due
    ON content.content_items(unpublish_at_utc)
    WHERE is_deleted = false AND status = 'Published' AND unpublish_at_utc IS NOT NULL;
""");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
DROP INDEX IF EXISTS content.ix_content_items_unpublish_due;
DROP INDEX IF EXISTS content.ix_content_items_scheduled_due;
""");
    }
}
