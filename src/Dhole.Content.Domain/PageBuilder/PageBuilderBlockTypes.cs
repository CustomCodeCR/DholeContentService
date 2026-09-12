namespace Dhole.Content.Domain.PageBuilder;

public static class PageBuilderBlockTypes
{
    public const string Hero = "Hero";
    public const string RichText = "RichText";
    public const string Image = "Image";
    public const string Video = "Video";
    public const string Gallery = "Gallery";
    public const string CTA = "CTA";
    public const string ServicesGrid = "ServicesGrid";
    public const string NewsGrid = "NewsGrid";
    public const string FAQ = "FAQ";
    public const string Testimonials = "Testimonials";
    public const string Logos = "Logos";
    public const string Stats = "Stats";
    public const string Team = "Team";
    public const string Banner = "Banner";
    public const string Form = "Form";
    public const string MeetingForm = "MeetingForm";

    public static IReadOnlyCollection<string> All =>
    [
        Hero,
        RichText,
        Image,
        Video,
        Gallery,
        CTA,
        ServicesGrid,
        NewsGrid,
        FAQ,
        Testimonials,
        Logos,
        Stats,
        Team,
        Banner,
        Form,
        MeetingForm
    ];

    public static bool IsSupported(string? value)
        => !string.IsNullOrWhiteSpace(value)
           && All.Contains(value.Trim(), StringComparer.OrdinalIgnoreCase);

    public static string Normalize(string value)
        => All.First(type => string.Equals(type, value.Trim(), StringComparison.OrdinalIgnoreCase));
}
