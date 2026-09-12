using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Content.Application.Abstractions.Repositories;
using Dhole.Content.Domain.Consents.Entities;
using Dhole.Content.Domain.Shared;

namespace Dhole.Content.Application.Consents;

public sealed record CaptureMarketingConsentCommand(
    Guid? LeadId,
    Guid? SubmissionId,
    string Purpose,
    bool Granted,
    string PolicyVersion,
    string Source,
    DateTime? CapturedAtUtc,
    Guid? ActorUserId) : ICommand<Result<Guid>>;

public sealed class CaptureMarketingConsentCommandHandler(
    IMarketingConsentRepository consents,
    IMarketingLeadRepository leads,
    IMarketingSubmissionRepository submissions,
    IMarketingFormRepository forms,
    IUnitOfWork unitOfWork) : ICommandHandler<CaptureMarketingConsentCommand, Result<Guid>>
{
    public async Task<Result<Guid>> HandleAsync(
        CaptureMarketingConsentCommand command,
        CancellationToken cancellationToken = default)
    {
        string? leadSiteKey = null;
        if (command.LeadId.HasValue)
        {
            var lead = await leads.GetByIdAsync(command.LeadId.Value, cancellationToken);
            if (lead is null || lead.IsDeleted)
                return Result.Failure<Guid>(ContentErrors.MarketingLeadNotFound);
            leadSiteKey = lead.SiteKey;
        }

        if (command.SubmissionId.HasValue)
        {
            var submission = await submissions.GetByIdAsync(command.SubmissionId.Value, cancellationToken);
            if (submission is null || submission.IsDeleted)
                return Result.Failure<Guid>(ContentErrors.MarketingSubmissionNotFound);

            if (leadSiteKey is not null)
            {
                var form = await forms.GetByIdAsync(submission.FormId, cancellationToken);
                if (form is null || form.IsDeleted ||
                    !string.Equals(leadSiteKey, form.SiteKey, StringComparison.Ordinal))
                    return Result.Failure<Guid>(ContentErrors.InvalidMarketingConsentData);
            }
        }

        MarketingConsent consent;
        try
        {
            consent = MarketingConsent.Create(command.LeadId, command.SubmissionId, command.Purpose,
                command.Granted, command.PolicyVersion, command.Source, command.CapturedAtUtc,
                command.ActorUserId, DateTime.UtcNow);
        }
        catch (ArgumentException)
        {
            return Result.Failure<Guid>(ContentErrors.InvalidMarketingConsentData);
        }

        await consents.AddAsync(consent, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(consent.Id);
    }
}
