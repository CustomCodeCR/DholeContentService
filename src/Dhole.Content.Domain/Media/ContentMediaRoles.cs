namespace Dhole.Content.Domain.Media;

public static class ContentMediaRoles
{
    public const string Hero = "hero";
    public const string Featured = "featured";
    public const string Gallery = "gallery";
    public const string Thumbnail = "thumbnail";
    public const string Background = "background";
    public const string Video = "video";
    public const string VideoPoster = "video.poster";
    public const string BannerDesktop = "banner.desktop";
    public const string BannerMobile = "banner.mobile";
    public const string OpenGraph = "open-graph";

    public static IReadOnlyCollection<string> All =>
    [
        Hero,
        Featured,
        Gallery,
        Thumbnail,
        Background,
        Video,
        VideoPoster,
        BannerDesktop,
        BannerMobile,
        OpenGraph
    ];

    public static bool IsSupported(string? role)
        => !string.IsNullOrWhiteSpace(role) && All.Contains(role.Trim().ToLowerInvariant(), StringComparer.Ordinal);

    public static string Normalize(string role)
    {
        var normalized = string.IsNullOrWhiteSpace(role)
            ? throw new ArgumentException("Role is required.", nameof(role))
            : role.Trim().ToLowerInvariant();

        return IsSupported(normalized)
            ? normalized
            : throw new ArgumentException($"Unsupported content media role: {role}.", nameof(role));
    }
}
