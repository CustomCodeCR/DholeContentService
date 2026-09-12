using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Content.Application.Abstractions.Repositories;
using Dhole.Content.Contracts.Consents;
using Dhole.Content.Contracts.Submissions;
using Dhole.Content.Domain.Consents.Entities;
using Dhole.Content.Domain.Forms;
using Dhole.Content.Domain.Shared;
using Dhole.Content.Domain.Submissions;
using Dhole.Content.Domain.Submissions.Entities;

namespace Dhole.Content.Application.Submissions;

public sealed record SubmitMarketingFormCommand(
    Guid FormId,
    Guid? ContentId,
    Guid? CampaignId,
    string SourceUrl,
    string? ReferrerUrl,
    string? UtmSource,
    string? UtmMedium,
    string? UtmCampaign,
    string? UtmContent,
    string? UtmTerm,
    string PayloadJson,
    string? IpHash,
    string? UserAgent,
    Guid CorrelationId,
    IReadOnlyCollection<SubmitMarketingConsentRequest>? Consents = null) : ICommand<Result<MarketingSubmissionReceiptDto>>;

public sealed class SubmitMarketingFormCommandHandler(
    IMarketingFormRepository forms,
    IMarketingFormFieldRepository fields,
    IMarketingSubmissionRepository submissions,
    IMarketingConsentRepository consents,
    IContentItemRepository contentItems,
    IMarketingCampaignRepository campaigns,
    IUnitOfWork unitOfWork) : ICommandHandler<SubmitMarketingFormCommand, Result<MarketingSubmissionReceiptDto>>
{
    public async Task<Result<MarketingSubmissionReceiptDto>> HandleAsync(
        SubmitMarketingFormCommand command,
        CancellationToken cancellationToken = default)
    {
        var form = await forms.GetByIdAsync(command.FormId, cancellationToken);
        if (form is null || form.IsDeleted)
            return Result.Failure<MarketingSubmissionReceiptDto>(ContentErrors.MarketingFormNotFound);
        if (!string.Equals(form.Status, MarketingFormRules.StatusActive, StringComparison.Ordinal))
            return Result.Failure<MarketingSubmissionReceiptDto>(ContentErrors.MarketingFormNotActive);

        if (command.ContentId.HasValue)
        {
            var content = await contentItems.GetByIdAsync(command.ContentId.Value, cancellationToken);
            if (content is null || content.IsDeleted)
                return Result.Failure<MarketingSubmissionReceiptDto>(ContentErrors.ContentNotFound);
            if (!string.Equals(content.SiteKey, form.SiteKey, StringComparison.Ordinal))
                return Result.Failure<MarketingSubmissionReceiptDto>(ContentErrors.InvalidMarketingSubmissionData);
        }

        if (command.CampaignId.HasValue)
        {
            var campaign = await campaigns.GetByIdAsync(command.CampaignId.Value, cancellationToken);
            if (campaign is null || campaign.IsDeleted || !string.Equals(campaign.SiteKey, form.SiteKey, StringComparison.Ordinal))
                return Result.Failure<MarketingSubmissionReceiptDto>(ContentErrors.InvalidMarketingSubmissionData);
        }

        string sanitizedPayload;
        try
        {
            var formFields = await fields.GetByFormAsync(form.Id, cancellationToken);
            sanitizedPayload = SubmissionRules.ValidateAndSanitizePayload(
                command.PayloadJson,
                formFields.Select(field => (field.FieldKey, field.IsRequired)));
        }
        catch (ArgumentException)
        {
            return Result.Failure<MarketingSubmissionReceiptDto>(ContentErrors.InvalidMarketingSubmissionData);
        }

        MarketingSubmission submission;
        try
        {
            submission = MarketingSubmission.Create(
                form.Id,
                command.ContentId,
                command.CampaignId,
                DateTime.UtcNow,
                command.SourceUrl,
                command.ReferrerUrl,
                command.UtmSource,
                command.UtmMedium,
                command.UtmCampaign,
                command.UtmContent,
                command.UtmTerm,
                sanitizedPayload,
                command.IpHash,
                command.UserAgent,
                command.CorrelationId);
        }
        catch (ArgumentException)
        {
            return Result.Failure<MarketingSubmissionReceiptDto>(ContentErrors.InvalidMarketingSubmissionData);
        }

        var consentEntities = new List<MarketingConsent>();
        if (command.Consents is { Count: > 0 })
        {
            var seenPurposes = new HashSet<string>(StringComparer.Ordinal);
            foreach (var input in command.Consents)
            {
                MarketingConsent consent;
                try
                {
                    consent = MarketingConsent.Create(null, submission.Id, input.Purpose, input.Granted,
                        input.PolicyVersion, input.Source ?? "form", submission.SubmittedAtUtc, null,
                        submission.SubmittedAtUtc);
                }
                catch (ArgumentException)
                {
                    return Result.Failure<MarketingSubmissionReceiptDto>(ContentErrors.InvalidMarketingConsentData);
                }

                if (!seenPurposes.Add(consent.Purpose))
                    return Result.Failure<MarketingSubmissionReceiptDto>(ContentErrors.InvalidMarketingConsentData);
                consentEntities.Add(consent);
            }
        }

        await submissions.AddAsync(submission, cancellationToken);
        foreach (var consent in consentEntities)
            await consents.AddAsync(consent, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new MarketingSubmissionReceiptDto(
            submission.Id,
            submission.SubmittedAtUtc,
            form.SuccessMessage));
    }
}
