using System.Net.Mail;

namespace Dhole.Content.Domain.Leads;

public static class MarketingLeadRules
{
    public const string DefaultStatus = "New";

    public static string NormalizeSiteKey(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("SiteKey is required.", nameof(value));
        var normalized = value.Trim().ToLowerInvariant();
        if (normalized.Length > 80) throw new ArgumentOutOfRangeException(nameof(value));
        return normalized;
    }

    public static string? NormalizeEmail(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var normalized = value.Trim().ToLowerInvariant();
        if (normalized.Length > 320) throw new ArgumentOutOfRangeException(nameof(value));
        try
        {
            var address = new MailAddress(normalized);
            if (!string.Equals(address.Address, normalized, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Email is invalid.", nameof(value));
            return address.Address.ToLowerInvariant();
        }
        catch (FormatException exception)
        {
            throw new ArgumentException("Email is invalid.", nameof(value), exception);
        }
    }

    public static string NormalizeStatus(string? value)
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? DefaultStatus : value.Trim();
        if (normalized.Length > 80) throw new ArgumentOutOfRangeException(nameof(value));
        return normalized;
    }

    public static string? NormalizeOptionalText(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var normalized = value.Trim();
        if (normalized.Length > maxLength) throw new ArgumentOutOfRangeException(nameof(value));
        return normalized;
    }
}
