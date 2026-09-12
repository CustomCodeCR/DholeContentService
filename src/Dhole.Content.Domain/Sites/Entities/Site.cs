using CustomCodeFramework.Core.Domain.Entities;

namespace Dhole.Content.Domain.Sites.Entities;

public sealed class Site : SoftDeletableAggregateRoot<Guid>
{
    private Site() { }

    private Site(
        Guid id,
        string siteKey,
        string name,
        string primaryDomain,
        string defaultLocale,
        string timeZone,
        Guid? logoMediaId,
        Guid? faviconMediaId,
        Guid? defaultOpenGraphMediaId,
        string status,
        Guid? actorUserId
    ) : base(id)
    {
        SiteKey = NormalizeSiteKey(siteKey);
        Name = Required(name, nameof(name));
        PrimaryDomain = NormalizeDomain(primaryDomain);
        DefaultLocale = Required(defaultLocale, nameof(defaultLocale));
        TimeZone = Required(timeZone, nameof(timeZone));
        LogoMediaId = logoMediaId;
        FaviconMediaId = faviconMediaId;
        DefaultOpenGraphMediaId = defaultOpenGraphMediaId;
        Status = Required(status, nameof(status));
        MarkAsCreated(DateTime.UtcNow, actorUserId?.ToString());
    }

    public string SiteKey { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string PrimaryDomain { get; private set; } = string.Empty;
    public string DefaultLocale { get; private set; } = "es-CR";
    public string TimeZone { get; private set; } = "America/Costa_Rica";
    public Guid? LogoMediaId { get; private set; }
    public Guid? FaviconMediaId { get; private set; }
    public Guid? DefaultOpenGraphMediaId { get; private set; }
    public string Status { get; private set; } = "Active";

    public static Site Create(
        string siteKey,
        string name,
        string primaryDomain,
        string defaultLocale,
        string timeZone,
        Guid? logoMediaId,
        Guid? faviconMediaId,
        Guid? defaultOpenGraphMediaId,
        string? status,
        Guid? actorUserId
    ) => new(
        Guid.NewGuid(),
        siteKey,
        name,
        primaryDomain,
        defaultLocale,
        timeZone,
        logoMediaId,
        faviconMediaId,
        defaultOpenGraphMediaId,
        string.IsNullOrWhiteSpace(status) ? "Active" : status,
        actorUserId
    );

    public void Update(
        string name,
        string primaryDomain,
        string defaultLocale,
        string timeZone,
        Guid? logoMediaId,
        Guid? faviconMediaId,
        Guid? defaultOpenGraphMediaId,
        string status,
        Guid? actorUserId
    )
    {
        Name = Required(name, nameof(name));
        PrimaryDomain = NormalizeDomain(primaryDomain);
        DefaultLocale = Required(defaultLocale, nameof(defaultLocale));
        TimeZone = Required(timeZone, nameof(timeZone));
        LogoMediaId = logoMediaId;
        FaviconMediaId = faviconMediaId;
        DefaultOpenGraphMediaId = defaultOpenGraphMediaId;
        Status = Required(status, nameof(status));
        MarkAsUpdated(DateTime.UtcNow, actorUserId?.ToString());
    }

    public void Delete(Guid? actorUserId)
        => MarkAsDeleted(DateTime.UtcNow, actorUserId?.ToString());

    private static string NormalizeSiteKey(string value)
        => Required(value, nameof(value)).ToLowerInvariant();

    private static string NormalizeDomain(string value)
    {
        var domain = Required(value, nameof(value)).ToLowerInvariant();
        if (domain.StartsWith("https://", StringComparison.OrdinalIgnoreCase)) domain = domain[8..];
        else if (domain.StartsWith("http://", StringComparison.OrdinalIgnoreCase)) domain = domain[7..];
        return domain.TrimEnd('/');
    }

    private static string Required(string value, string parameterName)
        => string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException("El valor es obligatorio.", parameterName)
            : value.Trim();
}
