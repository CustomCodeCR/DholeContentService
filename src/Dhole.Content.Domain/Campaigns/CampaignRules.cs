using System.Text.Json;
using System.Text.RegularExpressions;

namespace Dhole.Content.Domain.Campaigns;

public static partial class CampaignRules
{
    public static string NormalizeSiteKey(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("SiteKey is required.", nameof(value));
        var normalized = value.Trim().ToLowerInvariant();
        if (normalized.Length > 80) throw new ArgumentException("SiteKey is too long.", nameof(value));
        return normalized;
    }

    public static string NormalizeName(string value)
        => NormalizeRequired(value, 240, nameof(value));

    public static string NormalizeSlug(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Slug is required.", nameof(value));
        var normalized = value.Trim().ToLowerInvariant();
        if (normalized.Length > 160 || !SlugPattern().IsMatch(normalized))
            throw new ArgumentException("Slug has an invalid format.", nameof(value));
        return normalized;
    }

    public static string NormalizeStatus(string value)
        => NormalizeRequired(value, 80, nameof(value));

    public static string NormalizeGoalType(string value)
        => NormalizeRequired(value, 120, nameof(value));

    public static string? NormalizeUtm(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var normalized = value.Trim();
        if (normalized.Length > 240) throw new ArgumentException("UTM value is too long.", nameof(value));
        return normalized;
    }

    public static string? NormalizeSettingsJson(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        try
        {
            using var document = JsonDocument.Parse(value);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
                throw new ArgumentException("SettingsJson must be a JSON object.", nameof(value));
            return value.Trim();
        }
        catch (JsonException exception)
        {
            throw new ArgumentException("SettingsJson is invalid JSON.", nameof(value), exception);
        }
    }

    public static void ValidateWindow(DateTime? startsAtUtc, DateTime? endsAtUtc)
    {
        if (startsAtUtc.HasValue && endsAtUtc.HasValue && endsAtUtc.Value <= startsAtUtc.Value)
            throw new ArgumentException("EndsAtUtc must be after StartsAtUtc.", nameof(endsAtUtc));
    }

    private static string NormalizeRequired(string value, int maxLength, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Value is required.", parameterName);
        var normalized = value.Trim();
        if (normalized.Length > maxLength) throw new ArgumentException("Value is too long.", parameterName);
        return normalized;
    }

    [GeneratedRegex("^[a-z0-9]+(?:[.-][a-z0-9]+)*$", RegexOptions.CultureInvariant)]
    private static partial Regex SlugPattern();
}
