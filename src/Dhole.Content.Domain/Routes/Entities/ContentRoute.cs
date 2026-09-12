using CustomCodeFramework.Core.Domain.Entities;

namespace Dhole.Content.Domain.Routes.Entities;

public sealed class ContentRoute : SoftDeletableAggregateRoot<Guid>
{
    private ContentRoute() { }

    private ContentRoute(
        Guid id,
        string siteKey,
        Guid contentId,
        string locale,
        string path,
        bool isPrimary,
        bool isActive,
        Guid? actorUserId
    ) : base(id)
    {
        SiteKey = NormalizeSiteKey(siteKey);
        ContentId = contentId == Guid.Empty
            ? throw new ArgumentException("El contenido es obligatorio.", nameof(contentId))
            : contentId;
        Locale = Required(locale, nameof(locale));
        Path = NormalizePath(path);
        IsPrimary = isPrimary;
        IsActive = isActive;
        MarkAsCreated(DateTime.UtcNow, actorUserId?.ToString());
    }

    public string SiteKey { get; private set; } = "main";
    public Guid ContentId { get; private set; }
    public string Locale { get; private set; } = "es-CR";
    public string Path { get; private set; } = "/";
    public bool IsPrimary { get; private set; }
    public bool IsActive { get; private set; } = true;

    public static ContentRoute Create(
        string siteKey,
        Guid contentId,
        string locale,
        string path,
        bool isPrimary,
        bool isActive,
        Guid? actorUserId
    ) => new(Guid.NewGuid(), siteKey, contentId, locale, path, isPrimary, isActive, actorUserId);

    public void Update(
        string siteKey,
        string locale,
        string path,
        bool isPrimary,
        bool isActive,
        Guid? actorUserId
    )
    {
        SiteKey = NormalizeSiteKey(siteKey);
        Locale = Required(locale, nameof(locale));
        Path = NormalizePath(path);
        IsPrimary = isPrimary;
        IsActive = isActive;
        MarkAsUpdated(DateTime.UtcNow, actorUserId?.ToString());
    }

    public void SetPrimary(bool isPrimary, Guid? actorUserId)
    {
        if (IsPrimary == isPrimary) return;
        IsPrimary = isPrimary;
        MarkAsUpdated(DateTime.UtcNow, actorUserId?.ToString());
    }

    public void SetActive(bool isActive, Guid? actorUserId)
    {
        if (IsActive == isActive) return;
        IsActive = isActive;
        MarkAsUpdated(DateTime.UtcNow, actorUserId?.ToString());
    }

    public void Delete(Guid? actorUserId)
        => MarkAsDeleted(DateTime.UtcNow, actorUserId?.ToString());

    public static string NormalizePath(string value)
    {
        var path = Required(value, nameof(value)).Replace('\\', '/');
        if (!path.StartsWith('/')) path = "/" + path;

        while (path.Contains("//", StringComparison.Ordinal))
        {
            path = path.Replace("//", "/", StringComparison.Ordinal);
        }

        if (path.Length > 1) path = path.TrimEnd('/');
        return path.ToLowerInvariant();
    }

    private static string NormalizeSiteKey(string value)
        => Required(value, nameof(value)).ToLowerInvariant();

    private static string Required(string value, string parameterName)
        => string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException("El valor es obligatorio.", parameterName)
            : value.Trim();
}
