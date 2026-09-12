namespace Dhole.Content.Api.Authorization;

public static class ContentScopeNames
{
    public const string View = "cms.view";
    public const string Create = "cms.create";
    public const string Edit = "cms.edit";
    public const string Delete = "cms.delete";
    public const string Publish = "cms.publish";
    public const string MediaUpload = "cms.media.upload";
    public const string MediaDelete = "cms.media.delete";
    public const string PagesEdit = "cms.pages.edit";
    public const string NewsEdit = "cms.news.edit";
    public const string BannersEdit = "cms.banners.edit";
    public const string SeoEdit = "cms.seo.edit";
    public const string SettingsEdit = "cms.settings.edit";

    // FASE 23 — granular Marketing permissions.
    public const string NavigationEdit = "cms.navigation.edit";
    public const string CollectionsEdit = "cms.collections.edit";
    public const string FormsView = "cms.forms.view";
    public const string FormsEdit = "cms.forms.edit";
    public const string SubmissionsView = "cms.submissions.view";
    public const string LeadsView = "cms.leads.view";
    public const string LeadsEdit = "cms.leads.edit";
    public const string MeetingsView = "cms.meetings.view";
    public const string MeetingsEdit = "cms.meetings.edit";
    public const string CampaignsView = "cms.campaigns.view";
    public const string CampaignsEdit = "cms.campaigns.edit";
    public const string RedirectsEdit = "cms.redirects.edit";
    public const string ReviewsSubmit = "cms.reviews.submit";
    public const string ReviewsApprove = "cms.reviews.approve";

    public static IReadOnlyCollection<string> All =>
    [
        View, Create, Edit, Delete, Publish, MediaUpload, MediaDelete,
        PagesEdit, NewsEdit, BannersEdit, SeoEdit, SettingsEdit,
        NavigationEdit, CollectionsEdit, FormsView, FormsEdit, SubmissionsView,
        LeadsView, LeadsEdit, MeetingsView, MeetingsEdit, CampaignsView, CampaignsEdit,
        RedirectsEdit, ReviewsSubmit, ReviewsApprove,
    ];
}
