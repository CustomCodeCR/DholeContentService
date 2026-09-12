using System.Text.Json;
using Dhole.Content.Domain.ContentItems.Enums;

namespace Dhole.Content.Domain.Placements;

public static class PlacementRules
{
    public static string NormalizeCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Placement code is required.", nameof(code));

        var normalized = code.Trim().ToLowerInvariant();
        if (normalized.Length > 160 || normalized.Any(ch => !(char.IsLetterOrDigit(ch) || ch is '.' or '-' or '_')))
            throw new ArgumentException("Placement code contains unsupported characters.", nameof(code));

        return normalized;
    }

    public static string NormalizeAllowedTypesJson(string allowedTypesJson)
    {
        if (string.IsNullOrWhiteSpace(allowedTypesJson))
            throw new ArgumentException("AllowedTypesJson is required.", nameof(allowedTypesJson));

        try
        {
            using var document = JsonDocument.Parse(allowedTypesJson);
            if (document.RootElement.ValueKind != JsonValueKind.Array)
                throw new ArgumentException("AllowedTypesJson must be a JSON array.", nameof(allowedTypesJson));

            var types = new List<ContentType>();
            foreach (var element in document.RootElement.EnumerateArray())
            {
                if (element.ValueKind != JsonValueKind.String ||
                    !Enum.TryParse<ContentType>(element.GetString(), true, out var contentType))
                    throw new ArgumentException("AllowedTypesJson contains an unsupported content type.", nameof(allowedTypesJson));

                if (!types.Contains(contentType)) types.Add(contentType);
            }

            if (types.Count == 0)
                throw new ArgumentException("AllowedTypesJson must contain at least one content type.", nameof(allowedTypesJson));

            return JsonSerializer.Serialize(types.Select(type => type.ToString()));
        }
        catch (JsonException exception)
        {
            throw new ArgumentException("AllowedTypesJson must contain valid JSON.", nameof(allowedTypesJson), exception);
        }
    }

    public static bool AllowsContentType(string allowedTypesJson, ContentType contentType)
    {
        using var document = JsonDocument.Parse(allowedTypesJson);
        return document.RootElement.EnumerateArray().Any(element =>
            element.ValueKind == JsonValueKind.String &&
            Enum.TryParse<ContentType>(element.GetString(), true, out var parsed) &&
            parsed == contentType);
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
