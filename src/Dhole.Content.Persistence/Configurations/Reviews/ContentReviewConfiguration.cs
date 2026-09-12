using CustomCodeFramework.Postgres.EntityFramework.Configurations;
using Dhole.Content.Domain.ContentItems.Entities;
using Dhole.Content.Domain.Reviews.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dhole.Content.Persistence.Configurations.Reviews;

internal sealed class ContentReviewConfiguration : EntityTypeConfigurationBase<ContentReview, Guid>
{
    public override void Configure(EntityTypeBuilder<ContentReview> builder)
    {
        base.Configure(builder);
        builder.ToTable("content_reviews");
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(x => x.Comment).HasMaxLength(2000);
        builder.Property(x => x.SubmittedAtUtc).IsRequired();

        builder.HasOne<ContentItem>()
            .WithMany()
            .HasForeignKey(x => x.ContentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<ContentRevision>()
            .WithMany()
            .HasForeignKey(x => x.RevisionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.ContentId, x.SubmittedAtUtc })
            .HasDatabaseName("ix_content_reviews_content_submitted");
        builder.HasIndex(x => x.ContentId)
            .IsUnique()
            .HasDatabaseName("ux_content_reviews_pending_content")
            .HasFilter("is_deleted = false AND status = 'Pending'");
    }
}
