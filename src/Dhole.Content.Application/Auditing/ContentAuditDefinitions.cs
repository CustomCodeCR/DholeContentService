namespace Dhole.Content.Application.Auditing;

public static class ContentAuditIntegration
{
    public const string SourceService = "DholeContentService";
    public const string RegisterEventContract = "Dhole.AuditLogs.Contracts.AuditEvents.RegisterAuditEventRequest";
    public const string RegisteredMessage = "audit.event.registered";
    public const string RedisStream = "dhole.audit.events";
}

public static class ContentAuditActions
{
    public const string Created = "created";
    public const string Updated = "updated";
    public const string Deleted = "deleted";
    public const string Published = "published";
    public const string Scheduled = "scheduled";
    public const string Approved = "approved";
    public const string Rejected = "rejected";
    public const string StatusChanged = "status_changed";
    public const string Reordered = "reordered";
    public const string Restored = "restored";

    public static string ResolveMutation(bool statusChanged = false, bool reordered = false)
        => reordered ? Reordered : statusChanged ? StatusChanged : Updated;
}

public static class ContentAuditEntityTypes
{
    public const string ContentItem = "ContentItem";
    public const string ContentReview = "ContentReview";
    public const string TaxonomyTerm = "TaxonomyTerm";
    public const string MediaReference = "MediaReference";
    public const string ContentMedia = "ContentMedia";
    public const string NavigationMenu = "NavigationMenu";
    public const string SiteSetting = "SiteSetting";
    public const string Placement = "Placement";
    public const string PlacementItem = "PlacementItem";
    public const string Collection = "Collection";
    public const string CollectionItem = "CollectionItem";
    public const string MarketingForm = "MarketingForm";
    public const string MarketingFormField = "MarketingFormField";
    public const string MarketingSubmission = "MarketingSubmission";
    public const string MarketingConsent = "MarketingConsent";
    public const string MarketingLead = "MarketingLead";
    public const string MeetingType = "MeetingType";
    public const string MeetingRequest = "MeetingRequest";
    public const string MarketingCampaign = "MarketingCampaign";
    public const string Redirect = "Redirect";
}

public static class ContentAuditEventTypes
{
    public const string ContentCreated = "content.item.created";
    public const string ContentUpdated = "content.item.updated";
    public const string ContentReviewRequested = "content.item.review-requested";
    public const string ContentReviewSubmitted = "content.review.submitted";
    public const string ContentReviewApproved = "content.review.approved";
    public const string ContentReviewRejected = "content.review.rejected";
    public const string ContentPublished = "content.item.published";
    public const string ContentScheduled = "content.item.scheduled";
    public const string ContentUnpublished = "content.item.unpublished";
    public const string ContentArchived = "content.item.archived";
    public const string ContentDeleted = "content.item.deleted";
    public const string ContentRevisionRestored = "content.item.revision-restored";
    public const string SeoUpdated = "content.seo.updated";
    public const string TaxonomyCreated = "content.taxonomy.created";
    public const string TaxonomyUpdated = "content.taxonomy.updated";
    public const string TaxonomyDeleted = "content.taxonomy.deleted";
    public const string MediaCreated = "content.media.created";
    public const string MediaUpdated = "content.media.updated";
    public const string MediaDeleted = "content.media.deleted";
    public const string ContentMediaCreated = "content.media.linked";
    public const string ContentMediaUpdated = "content.media.link-updated";
    public const string ContentMediaDeleted = "content.media.unlinked";
    public const string MenuUpdated = "content.menu.updated";
    public const string SettingUpdated = "content.setting.updated";
    public const string SettingDeleted = "content.setting.deleted";
    public const string PlacementCreated = "content.placement.created";
    public const string PlacementUpdated = "content.placement.updated";
    public const string PlacementDeleted = "content.placement.deleted";
    public const string PlacementItemCreated = "content.placement-item.created";
    public const string PlacementItemUpdated = "content.placement-item.updated";
    public const string PlacementItemDeleted = "content.placement-item.deleted";
    public const string CollectionCreated = "content.collection.created";
    public const string CollectionUpdated = "content.collection.updated";
    public const string CollectionDeleted = "content.collection.deleted";
    public const string CollectionItemCreated = "content.collection-item.created";
    public const string CollectionItemUpdated = "content.collection-item.updated";
    public const string CollectionItemDeleted = "content.collection-item.deleted";
    public const string MarketingFormCreated = "content.marketing-form.created";
    public const string MarketingFormUpdated = "content.marketing-form.updated";
    public const string MarketingFormDeleted = "content.marketing-form.deleted";
    public const string MarketingFormFieldCreated = "content.marketing-form-field.created";
    public const string MarketingFormFieldUpdated = "content.marketing-form-field.updated";
    public const string MarketingFormFieldDeleted = "content.marketing-form-field.deleted";
    public const string MarketingSubmissionCreated = "content.marketing-submission.created";
    public const string MarketingConsentCreated = "content.marketing-consent.created";
    public const string MarketingLeadCreated = "content.marketing-lead.created";
    public const string MarketingLeadUpdated = "content.marketing-lead.updated";
    public const string MarketingLeadDeleted = "content.marketing-lead.deleted";
    public const string MeetingTypeCreated = "content.meeting-type.created";
    public const string MeetingTypeUpdated = "content.meeting-type.updated";
    public const string MeetingTypeDeleted = "content.meeting-type.deleted";
    public const string MeetingRequestCreated = "content.meeting-request.created";
    public const string MeetingRequestStatusChanged = "content.meeting-request.status-changed";
    public const string MarketingCampaignCreated = "content.marketing-campaign.created";
    public const string MarketingCampaignUpdated = "content.marketing-campaign.updated";
    public const string MarketingCampaignDeleted = "content.marketing-campaign.deleted";
    public const string RedirectCreated = "content.redirect.created";
    public const string RedirectUpdated = "content.redirect.updated";
    public const string RedirectDeleted = "content.redirect.deleted";
}
