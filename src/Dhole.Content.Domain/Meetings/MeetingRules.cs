using System.Text.Json;
using System.Text.RegularExpressions;

namespace Dhole.Content.Domain.Meetings;

public static class MeetingRules
{
    public const string StatusRequested = "Requested";
    public const string StatusPendingConfirmation = "PendingConfirmation";
    public const string StatusConfirmed = "Confirmed";
    public const string StatusRejected = "Rejected";
    public const string StatusCancelled = "Cancelled";
    public const string StatusCompleted = "Completed";

    private static readonly HashSet<string> Statuses = new(StringComparer.Ordinal)
    {
        StatusRequested, StatusPendingConfirmation, StatusConfirmed,
        StatusRejected, StatusCancelled, StatusCompleted
    };

    public static string NormalizeSiteKey(string value)
        => NormalizeRequired(value, 80, nameof(value)).ToLowerInvariant();

    public static string NormalizeSlug(string value)
    {
        var normalized = NormalizeRequired(value, 160, nameof(value)).ToLowerInvariant();
        if (!Regex.IsMatch(normalized, "^[a-z0-9]+(?:-[a-z0-9]+)*$"))
            throw new ArgumentException("Slug contains invalid characters.", nameof(value));
        return normalized;
    }

    public static string NormalizeRequired(string value, int maxLength, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Value is required.", parameterName);
        var normalized = value.Trim();
        if (normalized.Length > maxLength) throw new ArgumentException($"Value exceeds {maxLength} characters.", parameterName);
        return normalized;
    }

    public static string? NormalizeOptional(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var normalized = value.Trim();
        if (normalized.Length > maxLength) throw new ArgumentException($"Value exceeds {maxLength} characters.");
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
            return document.RootElement.GetRawText();
        }
        catch (JsonException exception)
        {
            throw new ArgumentException("SettingsJson must be valid JSON.", nameof(value), exception);
        }
    }

    public static void ValidateDuration(int durationMinutes, int bufferMinutes)
    {
        if (durationMinutes <= 0 || durationMinutes > 1440)
            throw new ArgumentOutOfRangeException(nameof(durationMinutes));
        if (bufferMinutes < 0 || bufferMinutes > 1440)
            throw new ArgumentOutOfRangeException(nameof(bufferMinutes));
    }

    public static void ValidateWindow(DateTime startUtc, DateTime endUtc)
    {
        if (endUtc <= startUtc) throw new ArgumentException("Meeting end must be after start.");
    }

    public static bool IsKnownStatus(string status) => Statuses.Contains(status);
}
