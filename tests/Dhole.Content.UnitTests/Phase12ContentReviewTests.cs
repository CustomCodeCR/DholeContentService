using Dhole.Content.Domain.ContentItems.Entities;
using Dhole.Content.Domain.ContentItems.Enums;
using Dhole.Content.Domain.Reviews.Entities;
using Dhole.Content.Domain.Reviews.Enums;
using Dhole.Content.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Dhole.Content.UnitTests;

public sealed class Phase12ContentReviewTests
{
    [Fact]
    public void ContentReview_SubmitAndApprove_TracksDecision()
    {
        var submittedAt = new DateTime(2026, 9, 12, 18, 0, 0, DateTimeKind.Utc);
        var decidedAt = submittedAt.AddMinutes(10);
        var submitter = Guid.NewGuid();
        var reviewer = Guid.NewGuid();
        var review = ContentReview.Submit(Guid.NewGuid(), Guid.NewGuid(), submitter, submittedAt);

        Assert.Equal(ContentReviewStatus.Pending, review.Status);
        Assert.Equal(submitter, review.SubmittedByUserId);
        Assert.Null(review.DecidedAtUtc);

        review.Approve(reviewer, "Listo para publicar", decidedAt);

        Assert.Equal(ContentReviewStatus.Approved, review.Status);
        Assert.Equal(reviewer, review.ReviewerUserId);
        Assert.Equal("Listo para publicar", review.Comment);
        Assert.Equal(decidedAt, review.DecidedAtUtc);
    }

    [Fact]
    public void ContentReview_Reject_CannotBeDecidedTwice()
    {
        var review = ContentReview.Submit(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow);
        review.Reject(Guid.NewGuid(), "Corregir contenido", DateTime.UtcNow.AddMinutes(1));

        Assert.Equal(ContentReviewStatus.Rejected, review.Status);
        Assert.Throws<InvalidOperationException>(() =>
            review.Approve(Guid.NewGuid(), null, DateTime.UtcNow.AddMinutes(2)));
    }

    [Fact]
    public void ContentItem_CanBeRejectedAndResubmitted()
    {
        var item = ContentItem.Create(ContentType.Page, "Página", "pagina", "[]", null, Guid.NewGuid());
        item.SubmitForReview(Guid.NewGuid());
        Assert.Equal(ContentStatus.PendingReview, item.Status);

        item.RejectReview(Guid.NewGuid());
        Assert.Equal(ContentStatus.Rejected, item.Status);

        item.SubmitForReview(Guid.NewGuid());
        Assert.Equal(ContentStatus.PendingReview, item.Status);
    }

    [Fact]
    public void ContentItem_PendingReview_CannotBeMutated()
    {
        var actor = Guid.NewGuid();
        var item = ContentItem.Create(ContentType.Page, "Página", "pagina", "[]", null, actor);
        item.SubmitForReview(actor);

        Assert.Throws<InvalidOperationException>(() =>
            item.Update("Página modificada", "pagina-modificada", null, "[]", null, null, 0, false, null, actor));
        Assert.Throws<InvalidOperationException>(() =>
            item.SetSeo("SEO nuevo", null, null, null, "index,follow", null, null, actor));
    }

    [Fact]
    public void ContentItem_Publish_OnlyAllowsPendingReviewOrScheduled()
    {
        var item = ContentItem.Create(ContentType.Page, "Página", "pagina", "[]", null, Guid.NewGuid());
        Assert.Throws<InvalidOperationException>(() => item.Publish(DateTime.UtcNow, Guid.NewGuid()));

        item.SubmitForReview(Guid.NewGuid());
        item.Publish(DateTime.UtcNow, Guid.NewGuid());
        Assert.Equal(ContentStatus.Published, item.Status);
    }

    [Fact]
    public void ContentReviewModel_HasPendingUniqueIndexAndForeignKeys()
    {
        var options = new DbContextOptionsBuilder<ServiceDbContext>()
            .UseNpgsql("Host=localhost;Database=dhole_content_phase12_test")
            .Options;
        using var db = new ServiceDbContext(options);

        var review = db.Model.FindEntityType(typeof(ContentReview));
        Assert.NotNull(review);

        var pendingIndex = review!.GetIndexes().Single(index =>
            index.Properties.Select(property => property.Name).SequenceEqual(["ContentId"]) && index.IsUnique);
        Assert.Equal("is_deleted = false AND status = 'Pending'", pendingIndex.GetFilter());

        Assert.Contains(review.GetForeignKeys(), fk =>
            fk.Properties.Select(property => property.Name).SequenceEqual(["ContentId"]));
        Assert.Contains(review.GetForeignKeys(), fk =>
            fk.Properties.Select(property => property.Name).SequenceEqual(["RevisionId"]));
    }

    [Fact]
    public void ServiceDbContext_ExposesContentReviews()
        => Assert.Contains("ContentReviews", typeof(ServiceDbContext).GetProperties().Select(property => property.Name));
}
