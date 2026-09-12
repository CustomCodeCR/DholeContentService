using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Queries;
using Dhole.Content.Application.Abstractions.Repositories;
using Dhole.Content.Contracts.Submissions;
using Dhole.Content.Domain.Shared;
using Dhole.Content.Domain.Submissions.Entities;

namespace Dhole.Content.Application.Submissions;

public sealed record GetMarketingSubmissionsQuery(
    Guid? FormId,
    string? Status,
    DateTime? SubmittedFromUtc,
    DateTime? SubmittedToUtc) : IQuery<IReadOnlyCollection<MarketingSubmissionDto>>;

public sealed record GetMarketingSubmissionByIdQuery(Guid Id) : IQuery<Result<MarketingSubmissionDto>>;

public sealed class GetMarketingSubmissionsQueryHandler(IMarketingSubmissionRepository submissions)
    : IQueryHandler<GetMarketingSubmissionsQuery, IReadOnlyCollection<MarketingSubmissionDto>>
{
    public async Task<IReadOnlyCollection<MarketingSubmissionDto>> HandleAsync(
        GetMarketingSubmissionsQuery query,
        CancellationToken cancellationToken = default)
        => (await submissions.GetAllAsync(query.FormId, query.Status, query.SubmittedFromUtc, query.SubmittedToUtc, cancellationToken))
            .Select(Map)
            .ToArray();

    internal static MarketingSubmissionDto Map(MarketingSubmission submission)
        => new(
            submission.Id,
            submission.FormId,
            submission.ContentId,
            submission.CampaignId,
            submission.SubmittedAtUtc,
            submission.Status,
            submission.SourceUrl,
            submission.ReferrerUrl,
            submission.UtmSource,
            submission.UtmMedium,
            submission.UtmCampaign,
            submission.UtmContent,
            submission.UtmTerm,
            submission.PayloadJson,
            submission.IpHash,
            submission.UserAgent,
            submission.CorrelationId);
}

public sealed class GetMarketingSubmissionByIdQueryHandler(IMarketingSubmissionRepository submissions)
    : IQueryHandler<GetMarketingSubmissionByIdQuery, Result<MarketingSubmissionDto>>
{
    public async Task<Result<MarketingSubmissionDto>> HandleAsync(
        GetMarketingSubmissionByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var submission = await submissions.GetByIdAsync(query.Id, cancellationToken);
        return submission is null || submission.IsDeleted
            ? Result.Failure<MarketingSubmissionDto>(ContentErrors.MarketingSubmissionNotFound)
            : Result.Success(GetMarketingSubmissionsQueryHandler.Map(submission));
    }
}
