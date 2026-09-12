using System.Text.Json;
using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Content.Application.Abstractions.Repositories;
using Dhole.Content.Contracts.Submissions;
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
    Guid CorrelationId) : ICommand<Result<MarketingSubmissionReceiptDto>>;

public sealed class SubmitMarketingFormCommandHandler(
    IMarketingFormRepository forms,
    IMarketingFormFieldRepository fields,
    IMarketingSubmissionRepository submissions,
    IContentItemRepository contentItems,
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

        string sanitizedPayload;
        try
        {
            sanitizedPayload = await ValidateAndSanitizePayloadAsync(form.Id, command.PayloadJson, cancellationToken);
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

        await submissions.AddAsync(submission, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new MarketingSubmissionReceiptDto(
            submission.Id,
            submission.SubmittedAtUtc,
            form.SuccessMessage));
    }

    private async Task<string> ValidateAndSanitizePayloadAsync(
        Guid formId,
        string payloadJson,
        CancellationToken cancellationToken)
    {
        using var document = JsonDocument.Parse(payloadJson);
        if (document.RootElement.ValueKind != JsonValueKind.Object)
            throw new ArgumentException("Payload must be a JSON object.", nameof(payloadJson));

        var formFields = await fields.GetByFormAsync(formId, cancellationToken);
        var definitions = formFields.ToDictionary(field => field.FieldKey, StringComparer.OrdinalIgnoreCase);
        var sanitized = new Dictionary<string, JsonElement>(StringComparer.Ordinal);

        foreach (var property in document.RootElement.EnumerateObject())
        {
            if (!definitions.TryGetValue(property.Name, out var definition))
                throw new ArgumentException("Payload contains a field that is not declared by the form.", nameof(payloadJson));
            sanitized[definition.FieldKey] = property.Value.Clone();
        }

        foreach (var field in formFields.Where(field => field.IsRequired))
        {
            if (!sanitized.TryGetValue(field.FieldKey, out var value) || !SubmissionRules.HasMeaningfulValue(value))
                throw new ArgumentException($"Required field '{field.FieldKey}' is missing.", nameof(payloadJson));
        }

        return JsonSerializer.Serialize(sanitized);
    }
}
