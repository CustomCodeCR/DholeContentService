namespace Dhole.Content.Domain.Consents;

public static class MarketingConsentRules
{
    public const string PurposePrivacy = "privacy";
    public const string PurposeContact = "contact";
    public const string PurposeNewsletter = "newsletter";
    public const string PurposeCommercialCommunications = "commercial-communications";

    public static IReadOnlyCollection<string> SupportedPurposes { get; } =
    [
        PurposePrivacy,
        PurposeContact,
        PurposeNewsletter,
        PurposeCommercialCommunications
    ];

    public static string NormalizePurpose(string value)
    {
        var normalized = NormalizeRequiredText(value, 80).ToLowerInvariant();
        normalized = normalized switch
        {
            "privacidad" => PurposePrivacy,
            "contacto" => PurposeContact,
            "comunicaciones comerciales" => PurposeCommercialCommunications,
            "commercial communications" => PurposeCommercialCommunications,
            _ => normalized.Replace('_', '-').Replace(' ', '-')
        };

        if (!SupportedPurposes.Contains(normalized, StringComparer.Ordinal))
            throw new ArgumentException("Unsupported consent purpose.", nameof(value));
        return normalized;
    }

    public static string NormalizePolicyVersion(string value)
        => NormalizeRequiredText(value, 80);

    public static string NormalizeSource(string value)
        => NormalizeRequiredText(value, 160).ToLowerInvariant();

    public static DateTime NormalizeCapturedAtUtc(DateTime? value, DateTime utcNow)
    {
        var now = ToUtc(utcNow);
        var captured = ToUtc(value ?? now);
        if (captured > now)
            throw new ArgumentException("CapturedAtUtc cannot be in the future.", nameof(value));
        return captured;
    }

    private static string NormalizeRequiredText(string value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("A value is required.", nameof(value));
        var normalized = value.Trim();
        if (normalized.Length > maxLength)
            throw new ArgumentException($"Value cannot exceed {maxLength} characters.", nameof(value));
        return normalized;
    }

    private static DateTime ToUtc(DateTime value)
        => value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
}
