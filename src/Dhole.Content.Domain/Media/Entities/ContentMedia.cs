using System.Text.Json;
using CustomCodeFramework.Core.Domain.Entities;

namespace Dhole.Content.Domain.Media.Entities;

public sealed class ContentMedia : SoftDeletableAggregateRoot<Guid>
{
    private ContentMedia() { }

    private ContentMedia(
        Guid id,
        Guid contentId,
        Guid mediaReferenceId,
        string role,
        int sortOrder,
        string? altTextOverride,
        string? captionOverride,
        decimal? focalX,
        decimal? focalY,
        string? settingsJson,
        Guid? actorUserId) : base(id)
    {
        ContentId = RequiredId(contentId, nameof(contentId));
        MediaReferenceId = RequiredId(mediaReferenceId, nameof(mediaReferenceId));
        Role = ContentMediaRoles.Normalize(role);
        ApplyMetadata(sortOrder, altTextOverride, captionOverride, focalX, focalY, settingsJson);
        MarkAsCreated(DateTime.UtcNow, actorUserId?.ToString());
    }

    public Guid ContentId { get; private set; }
    public Guid MediaReferenceId { get; private set; }
    public string Role { get; private set; } = ContentMediaRoles.Gallery;
    public int SortOrder { get; private set; }
    public string? AltTextOverride { get; private set; }
    public string? CaptionOverride { get; private set; }
    public decimal? FocalX { get; private set; }
    public decimal? FocalY { get; private set; }
    public string? SettingsJson { get; private set; }

    public static ContentMedia Create(
        Guid contentId,
        Guid mediaReferenceId,
        string role,
        int sortOrder,
        string? altTextOverride,
        string? captionOverride,
        decimal? focalX,
        decimal? focalY,
        string? settingsJson,
        Guid? actorUserId)
        => new(
            Guid.NewGuid(),
            contentId,
            mediaReferenceId,
            role,
            sortOrder,
            altTextOverride,
            captionOverride,
            focalX,
            focalY,
            settingsJson,
            actorUserId);

    public void Update(
        string role,
        int sortOrder,
        string? altTextOverride,
        string? captionOverride,
        decimal? focalX,
        decimal? focalY,
        string? settingsJson,
        Guid? actorUserId)
    {
        Role = ContentMediaRoles.Normalize(role);
        ApplyMetadata(sortOrder, altTextOverride, captionOverride, focalX, focalY, settingsJson);
        MarkAsUpdated(DateTime.UtcNow, actorUserId?.ToString());
    }

    public void Delete(Guid? actorUserId)
        => MarkAsDeleted(DateTime.UtcNow, actorUserId?.ToString());

    private void ApplyMetadata(
        int sortOrder,
        string? altTextOverride,
        string? captionOverride,
        decimal? focalX,
        decimal? focalY,
        string? settingsJson)
    {
        if (sortOrder < 0)
            throw new ArgumentOutOfRangeException(nameof(sortOrder), "SortOrder cannot be negative.");

        ValidateFocalPoint(focalX, nameof(focalX));
        ValidateFocalPoint(focalY, nameof(focalY));

        SortOrder = sortOrder;
        AltTextOverride = Normalize(altTextOverride);
        CaptionOverride = Normalize(captionOverride);
        FocalX = focalX;
        FocalY = focalY;
        SettingsJson = NormalizeSettingsJson(settingsJson);
    }

    private static Guid RequiredId(Guid value, string parameterName)
        => value == Guid.Empty
            ? throw new ArgumentException("A non-empty id is required.", parameterName)
            : value;

    private static void ValidateFocalPoint(decimal? value, string parameterName)
    {
        if (value is < 0m or > 1m)
            throw new ArgumentOutOfRangeException(parameterName, "Focal point values must be between 0 and 1.");
    }

    private static string? NormalizeSettingsJson(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;

        try
        {
            using var document = JsonDocument.Parse(value);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
                throw new ArgumentException("SettingsJson must be a JSON object.", nameof(value));
            return document.RootElement.GetRawText();
        }
        catch (JsonException ex)
        {
            throw new ArgumentException("SettingsJson must contain valid JSON.", nameof(value), ex);
        }
    }

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
