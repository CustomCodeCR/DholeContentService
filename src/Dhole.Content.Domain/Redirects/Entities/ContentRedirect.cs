using CustomCodeFramework.Core.Domain.Entities;

namespace Dhole.Content.Domain.Redirects.Entities;

public sealed class ContentRedirect : SoftDeletableAggregateRoot<Guid>
{
    private ContentRedirect() { }

    private ContentRedirect(Guid id, string siteKey, string sourcePath, string targetUrl, int statusCode,
        bool isActive, DateTime? validFromUtc, DateTime? validToUtc, Guid? actorUserId) : base(id)
    {
        Apply(siteKey, sourcePath, targetUrl, statusCode, isActive, validFromUtc, validToUtc);
        MarkAsCreated(DateTime.UtcNow, actorUserId?.ToString());
    }

    public string SiteKey { get; private set; } = "main";
    public string SourcePath { get; private set; } = "/";
    public string TargetUrl { get; private set; } = "/";
    public int StatusCode { get; private set; } = 301;
    public bool IsActive { get; private set; } = true;
    public DateTime? ValidFromUtc { get; private set; }
    public DateTime? ValidToUtc { get; private set; }

    public static ContentRedirect Create(string siteKey, string sourcePath, string targetUrl, int statusCode,
        bool isActive, DateTime? validFromUtc, DateTime? validToUtc, Guid? actorUserId)
        => new(Guid.NewGuid(), siteKey, sourcePath, targetUrl, statusCode, isActive, validFromUtc, validToUtc, actorUserId);

    public void Update(string siteKey, string sourcePath, string targetUrl, int statusCode,
        bool isActive, DateTime? validFromUtc, DateTime? validToUtc, Guid? actorUserId)
    {
        Apply(siteKey, sourcePath, targetUrl, statusCode, isActive, validFromUtc, validToUtc);
        MarkAsUpdated(DateTime.UtcNow, actorUserId?.ToString());
    }

    public void Delete(Guid? actorUserId) => MarkAsDeleted(DateTime.UtcNow, actorUserId?.ToString());

    public bool IsEffectiveAt(DateTime utcNow)
        => IsActive && !IsDeleted &&
           (!ValidFromUtc.HasValue || utcNow >= ValidFromUtc.Value) &&
           (!ValidToUtc.HasValue || utcNow < ValidToUtc.Value);

    public static string NormalizeSiteKey(string value)
        => Required(value, nameof(value)).ToLowerInvariant();

    public static string NormalizeSourcePath(string value)
    {
        var path = Required(value, nameof(value)).Replace('\\', '/');
        if (!path.StartsWith('/')) path = "/" + path;
        while (path.Contains("//", StringComparison.Ordinal))
            path = path.Replace("//", "/", StringComparison.Ordinal);
        if (path.Length > 1) path = path.TrimEnd('/');
        return path.ToLowerInvariant();
    }

    public static string NormalizeTargetUrl(string value)
    {
        var target = Required(value, nameof(value));
        if (target.Contains('\r') || target.Contains('\n'))
            throw new ArgumentException("TargetUrl contiene caracteres no permitidos.", nameof(value));

        if (target.StartsWith('/')) return target;
        if (!Uri.TryCreate(target, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            throw new ArgumentException("TargetUrl debe ser una ruta relativa o una URL HTTP/HTTPS.", nameof(value));
        return target;
    }

    private void Apply(string siteKey, string sourcePath, string targetUrl, int statusCode,
        bool isActive, DateTime? validFromUtc, DateTime? validToUtc)
    {
        if (statusCode is not (301 or 302))
            throw new ArgumentOutOfRangeException(nameof(statusCode), "StatusCode debe ser 301 o 302.");
        if (validFromUtc.HasValue && validToUtc.HasValue && validToUtc.Value <= validFromUtc.Value)
            throw new ArgumentException("ValidToUtc debe ser posterior a ValidFromUtc.", nameof(validToUtc));

        var source = NormalizeSourcePath(sourcePath);
        var target = NormalizeTargetUrl(targetUrl);
        if (target.StartsWith('/'))
        {
            var pathOnly = target.Split('?', '#')[0];
            if (NormalizeSourcePath(pathOnly) == source)
                throw new ArgumentException("Un redirect no puede apuntar a la misma ruta de origen.", nameof(targetUrl));
        }

        SiteKey = NormalizeSiteKey(siteKey);
        SourcePath = source;
        TargetUrl = target;
        StatusCode = statusCode;
        IsActive = isActive;
        ValidFromUtc = validFromUtc;
        ValidToUtc = validToUtc;
    }

    private static string Required(string value, string parameterName)
        => string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException("El valor es obligatorio.", parameterName)
            : value.Trim();
}
