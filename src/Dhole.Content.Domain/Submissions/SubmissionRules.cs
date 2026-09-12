using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Dhole.Content.Domain.Submissions;

public static class SubmissionRules
{
    public const string StatusReceived = "Received";

    public static string NormalizePayloadJson(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("PayloadJson is required.", nameof(value));
        try
        {
            using var document = JsonDocument.Parse(value);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
                throw new ArgumentException("PayloadJson must be a JSON object.", nameof(value));
            return value.Trim();
        }
        catch (JsonException exception)
        {
            throw new ArgumentException("PayloadJson is invalid.", nameof(value), exception);
        }
    }

    public static string ValidateAndSanitizePayload(
        string payloadJson,
        IEnumerable<(string FieldKey, bool IsRequired)> fieldDefinitions)
    {
        using var document = JsonDocument.Parse(payloadJson);
        if (document.RootElement.ValueKind != JsonValueKind.Object)
            throw new ArgumentException("Payload must be a JSON object.", nameof(payloadJson));

        var definitions = fieldDefinitions.ToDictionary(
            definition => definition.FieldKey,
            definition => definition,
            StringComparer.OrdinalIgnoreCase);
        var sanitized = new Dictionary<string, JsonElement>(StringComparer.Ordinal);

        foreach (var property in document.RootElement.EnumerateObject())
        {
            if (!definitions.TryGetValue(property.Name, out var definition))
                throw new ArgumentException("Payload contains a field that is not declared by the form.", nameof(payloadJson));
            sanitized[definition.FieldKey] = property.Value.Clone();
        }

        foreach (var definition in definitions.Values.Where(definition => definition.IsRequired))
        {
            if (!sanitized.TryGetValue(definition.FieldKey, out var value) || !HasMeaningfulValue(value))
                throw new ArgumentException($"Required field '{definition.FieldKey}' is missing.", nameof(payloadJson));
        }

        return JsonSerializer.Serialize(sanitized);
    }

    public static string? NormalizeOptionalText(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var normalized = value.Trim();
        if (normalized.Length > maxLength) throw new ArgumentOutOfRangeException(nameof(value));
        return normalized;
    }

    public static string? NormalizeIpHash(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var normalized = value.Trim().ToLowerInvariant();
        if (normalized.Length != 64 || normalized.Any(c => !Uri.IsHexDigit(c)))
            throw new ArgumentException("IpHash must be a SHA-256 hex value.", nameof(value));
        return normalized;
    }

    public static string? HashIp(string? ipAddress)
    {
        if (string.IsNullOrWhiteSpace(ipAddress)) return null;
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(ipAddress.Trim()));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    public static bool HasMeaningfulValue(JsonElement value)
        => value.ValueKind switch
        {
            JsonValueKind.Null or JsonValueKind.Undefined => false,
            JsonValueKind.String => !string.IsNullOrWhiteSpace(value.GetString()),
            JsonValueKind.Array => value.GetArrayLength() > 0,
            _ => true
        };
}
