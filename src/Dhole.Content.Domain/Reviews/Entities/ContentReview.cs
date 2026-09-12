using CustomCodeFramework.Core.Domain.Entities;
using Dhole.Content.Domain.Reviews.Enums;

namespace Dhole.Content.Domain.Reviews.Entities;

public sealed class ContentReview : SoftDeletableAggregateRoot<Guid>
{
    private ContentReview() { }

    private ContentReview(Guid id, Guid contentId, Guid revisionId, Guid? submittedByUserId, DateTime submittedAtUtc)
        : base(id)
    {
        if (contentId == Guid.Empty) throw new ArgumentException("ContentId es requerido.", nameof(contentId));
        if (revisionId == Guid.Empty) throw new ArgumentException("RevisionId es requerido.", nameof(revisionId));
        ContentId = contentId;
        RevisionId = revisionId;
        SubmittedByUserId = submittedByUserId;
        SubmittedAtUtc = submittedAtUtc;
        Status = ContentReviewStatus.Pending;
        MarkAsCreated(submittedAtUtc, submittedByUserId?.ToString());
    }

    public Guid ContentId { get; private set; }
    public Guid RevisionId { get; private set; }
    public Guid? SubmittedByUserId { get; private set; }
    public Guid? ReviewerUserId { get; private set; }
    public ContentReviewStatus Status { get; private set; }
    public string? Comment { get; private set; }
    public DateTime SubmittedAtUtc { get; private set; }
    public DateTime? DecidedAtUtc { get; private set; }

    public static ContentReview Submit(Guid contentId, Guid revisionId, Guid? submittedByUserId, DateTime submittedAtUtc)
        => new(Guid.NewGuid(), contentId, revisionId, submittedByUserId, submittedAtUtc);

    public void Approve(Guid? reviewerUserId, string? comment, DateTime decidedAtUtc)
    {
        EnsurePending();
        ReviewerUserId = reviewerUserId;
        Comment = NormalizeComment(comment);
        Status = ContentReviewStatus.Approved;
        DecidedAtUtc = decidedAtUtc;
        MarkAsUpdated(decidedAtUtc, reviewerUserId?.ToString());
    }

    public void Reject(Guid? reviewerUserId, string? comment, DateTime decidedAtUtc)
    {
        EnsurePending();
        ReviewerUserId = reviewerUserId;
        Comment = NormalizeComment(comment);
        Status = ContentReviewStatus.Rejected;
        DecidedAtUtc = decidedAtUtc;
        MarkAsUpdated(decidedAtUtc, reviewerUserId?.ToString());
    }

    private void EnsurePending()
    {
        if (Status != ContentReviewStatus.Pending)
            throw new InvalidOperationException("La revisión ya fue decidida.");
    }

    private static string? NormalizeComment(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var normalized = value.Trim();
        if (normalized.Length > 2000) throw new ArgumentOutOfRangeException(nameof(value), "El comentario no puede exceder 2000 caracteres.");
        return normalized;
    }
}
