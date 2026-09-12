namespace Dhole.Content.Contracts.Reviews;

public sealed record ContentReviewDto(
    Guid Id,
    Guid ContentId,
    Guid RevisionId,
    Guid? SubmittedByUserId,
    Guid? ReviewerUserId,
    string Status,
    string? Comment,
    DateTime SubmittedAtUtc,
    DateTime? DecidedAtUtc
);

public sealed record DecideContentReviewRequest(string? Comment);
