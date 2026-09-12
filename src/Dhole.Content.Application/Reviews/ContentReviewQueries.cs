using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Queries;
using Dhole.Content.Application.Abstractions.Repositories;
using Dhole.Content.Contracts.Reviews;
using Dhole.Content.Domain.Reviews.Entities;
using Dhole.Content.Domain.Reviews.Enums;
using Dhole.Content.Domain.Shared;

namespace Dhole.Content.Application.Reviews;

public sealed record GetContentReviewsQuery(Guid? ContentId, ContentReviewStatus? Status) : IQuery<IReadOnlyCollection<ContentReviewDto>>;
public sealed record GetContentReviewByIdQuery(Guid Id) : IQuery<Result<ContentReviewDto>>;

public sealed class GetContentReviewsQueryHandler(IContentReviewRepository reviews)
    : IQueryHandler<GetContentReviewsQuery, IReadOnlyCollection<ContentReviewDto>>
{
    public async Task<IReadOnlyCollection<ContentReviewDto>> HandleAsync(GetContentReviewsQuery query, CancellationToken cancellationToken = default)
        => (await reviews.GetAllAsync(query.ContentId, query.Status, cancellationToken)).Select(Map).ToArray();

    internal static ContentReviewDto Map(ContentReview review)
        => new(
            review.Id,
            review.ContentId,
            review.RevisionId,
            review.SubmittedByUserId,
            review.ReviewerUserId,
            review.Status.ToString(),
            review.Comment,
            review.SubmittedAtUtc,
            review.DecidedAtUtc);
}

public sealed class GetContentReviewByIdQueryHandler(IContentReviewRepository reviews)
    : IQueryHandler<GetContentReviewByIdQuery, Result<ContentReviewDto>>
{
    public async Task<Result<ContentReviewDto>> HandleAsync(GetContentReviewByIdQuery query, CancellationToken cancellationToken = default)
    {
        var review = await reviews.GetByIdAsync(query.Id, cancellationToken);
        return review is null || review.IsDeleted
            ? Result.Failure<ContentReviewDto>(ContentErrors.ContentReviewNotFound)
            : Result.Success(GetContentReviewsQueryHandler.Map(review));
    }
}
