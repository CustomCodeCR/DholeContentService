using System.Text.Json;

namespace Dhole.Content.Domain.Collections;

public static class CollectionRules
{
    public static string NormalizeCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Collection code is required.", nameof(code));

        var normalized = code.Trim().ToLowerInvariant();
        if (normalized.Length > 160 || normalized.Any(ch => !(char.IsLetterOrDigit(ch) || ch is '.' or '-' or '_')))
            throw new ArgumentException("Collection code contains unsupported characters.", nameof(code));

        return normalized;
    }

    public static string NormalizeDataJson(string dataJson)
    {
        if (string.IsNullOrWhiteSpace(dataJson))
            throw new ArgumentException("DataJson is required.", nameof(dataJson));

        try
        {
            using var document = JsonDocument.Parse(dataJson);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
                throw new ArgumentException("DataJson must be a JSON object.", nameof(dataJson));
            return document.RootElement.GetRawText();
        }
        catch (JsonException exception)
        {
            throw new ArgumentException("DataJson must contain valid JSON.", nameof(dataJson), exception);
        }
    }

    public static string? NormalizeSettingsJson(string? settingsJson)
    {
        if (string.IsNullOrWhiteSpace(settingsJson)) return null;

        try
        {
            using var document = JsonDocument.Parse(settingsJson);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
                throw new ArgumentException("SettingsJson must be a JSON object.", nameof(settingsJson));
            return document.RootElement.GetRawText();
        }
        catch (JsonException exception)
        {
            throw new ArgumentException("SettingsJson must contain valid JSON.", nameof(settingsJson), exception);
        }
    }
}
