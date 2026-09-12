using System.Text.Json;
using System.Text.RegularExpressions;

namespace Dhole.Content.Domain.Forms;

public static partial class MarketingFormRules
{
    public const string PurposeContact = "contact";
    public const string PurposeQuoteRequest = "quote-request";
    public const string PurposeMeeting = "meeting";
    public const string PurposeNewsletter = "newsletter";
    public const string PurposeCampaign = "campaign";

    public const string StatusDraft = "Draft";
    public const string StatusActive = "Active";
    public const string StatusInactive = "Inactive";

    public static readonly IReadOnlyCollection<string> InitialPurposes =
    [
        PurposeContact,
        PurposeQuoteRequest,
        PurposeMeeting,
        PurposeNewsletter,
        PurposeCampaign
    ];

    public static string NormalizeFormKey(string value) => NormalizeKey(value, nameof(value));
    public static string NormalizeFieldKey(string value) => NormalizeKey(value, nameof(value));
    public static string NormalizeFieldType(string value) => NormalizeKey(value, nameof(value), 80);

    public static string NormalizePurpose(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Purpose is required.", nameof(value));
        var normalized = value.Trim().ToLowerInvariant();
        if (!InitialPurposes.Contains(normalized, StringComparer.Ordinal))
            throw new ArgumentException("Purpose is not supported.", nameof(value));
        return normalized;
    }

    public static string NormalizeStatus(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Status is required.", nameof(value));
        return value.Trim().ToLowerInvariant() switch
        {
            "draft" => StatusDraft,
            "active" => StatusActive,
            "inactive" => StatusInactive,
            _ => throw new ArgumentException("Status is not supported.", nameof(value))
        };
    }

    public static string? NormalizeObjectJson(string? value, string parameterName)
        => NormalizeJson(value, parameterName, JsonValueKind.Object);

    public static string? NormalizeArrayJson(string? value, string parameterName)
        => NormalizeJson(value, parameterName, JsonValueKind.Array);

    public static string? NormalizeOptionalText(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string NormalizeKey(string value, string parameterName, int maxLength = 160)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Key is required.", parameterName);
        var normalized = value.Trim().ToLowerInvariant();
        if (normalized.Length > maxLength || !KeyPattern().IsMatch(normalized))
            throw new ArgumentException("Key has an invalid format.", parameterName);
        return normalized;
    }

    private static string? NormalizeJson(string? value, string parameterName, JsonValueKind expectedKind)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        try
        {
            using var document = JsonDocument.Parse(value);
            if (document.RootElement.ValueKind != expectedKind)
                throw new ArgumentException("JSON has an invalid root type.", parameterName);
            return value.Trim();
        }
        catch (JsonException exception)
        {
            throw new ArgumentException("JSON is invalid.", parameterName, exception);
        }
    }

    [GeneratedRegex("^[a-z0-9]+(?:[.-][a-z0-9]+)*$", RegexOptions.CultureInvariant)]
    private static partial Regex KeyPattern();
}
