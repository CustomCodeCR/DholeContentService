using Dhole.Content.Application.ContentItems.PublishDueContent;
using Dhole.Content.Domain.ContentItems.Entities;
using Dhole.Content.Domain.ContentItems.Enums;
using Dhole.Content.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Dhole.Content.UnitTests;

public sealed class Phase13ScheduledPublicationTests
{
    [Fact]
    public void Schedule_RequiresPendingReviewAndStoresPublicationWindow()
    {
        var actor = Guid.NewGuid();
        var item = ContentItem.Create(ContentType.Page, "Programada", "programada", "[]", null, actor);
        var now = new DateTime(2026, 9, 12, 18, 0, 0, DateTimeKind.Utc);
        var publishAt = now.AddHours(2);
        var unpublishAt = publishAt.AddDays(7);

        Assert.Throws<InvalidOperationException>(() => item.Schedule(publishAt, unpublishAt, now, actor));

        item.SubmitForReview(actor);
        item.Schedule(publishAt, unpublishAt, now, actor);

        Assert.Equal(ContentStatus.Scheduled, item.Status);
        Assert.Equal(publishAt, item.ScheduledAtUtc);
        Assert.Equal(unpublishAt, item.UnpublishAtUtc);
    }

    [Fact]
    public void Schedule_RejectsInvalidUnpublishWindow()
    {
        var actor = Guid.NewGuid();
        var item = ContentItem.Create(ContentType.News, "Noticia", "noticia", "[]", null, actor);
        var now = new DateTime(2026, 9, 12, 18, 0, 0, DateTimeKind.Utc);
        var publishAt = now.AddHours(1);
        item.SubmitForReview(actor);

        Assert.Throws<ArgumentException>(() => item.Schedule(publishAt, publishAt, now, actor));
    }

    [Fact]
    public void AutomaticUnpublishPath_ClearsUnpublishDateAndReturnsToDraft()
    {
        var actor = Guid.NewGuid();
        var item = ContentItem.Create(ContentType.Post, "Post", "post", "[]", null, actor);
        var now = new DateTime(2026, 9, 12, 18, 0, 0, DateTimeKind.Utc);
        var publishAt = now.AddMinutes(30);
        var unpublishAt = publishAt.AddHours(2);

        item.SubmitForReview(actor);
        item.Schedule(publishAt, unpublishAt, now, actor);
        item.Publish(publishAt, null);
        item.Unpublish(null);

        Assert.Equal(ContentStatus.Draft, item.Status);
        Assert.Null(item.ScheduledAtUtc);
        Assert.Null(item.UnpublishAtUtc);
    }

    [Fact]
    public void ProcessingResult_TracksBothDirections()
    {
        var result = new ScheduledContentProcessingResult(3, 2);
        Assert.Equal(3, result.PublishedCount);
        Assert.Equal(2, result.UnpublishedCount);
    }

    [Fact]
    public void ContentItemModel_HasDueDateIndexesForWorker()
    {
        var options = new DbContextOptionsBuilder<ServiceDbContext>()
            .UseNpgsql("Host=localhost;Database=dhole_content_phase13_test")
            .Options;
        using var db = new ServiceDbContext(options);

        var content = db.Model.FindEntityType(typeof(ContentItem));
        Assert.NotNull(content);

        var scheduled = content!.GetIndexes().Single(x => x.GetDatabaseName() == "ix_content_items_scheduled_due");
        Assert.Equal("is_deleted = false AND status = 'Scheduled' AND scheduled_at_utc IS NOT NULL", scheduled.GetFilter());

        var unpublish = content.GetIndexes().Single(x => x.GetDatabaseName() == "ix_content_items_unpublish_due");
        Assert.Equal("is_deleted = false AND status = 'Published' AND unpublish_at_utc IS NOT NULL", unpublish.GetFilter());
    }
}
