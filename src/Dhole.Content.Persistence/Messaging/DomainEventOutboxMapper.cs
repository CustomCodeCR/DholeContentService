using CustomCodeFramework.Core.Domain.Events;
using Dhole.Content.Domain.ContentItems.Events;
using Dhole.Content.Domain.Leads.Events;
using Dhole.Content.Domain.Media.Events;
using Dhole.Content.Domain.Navigation.Events;
using Dhole.Content.Domain.Settings.Events;
using Dhole.Content.Domain.Taxonomies.Events;
namespace Dhole.Content.Persistence.Messaging;
internal static class DomainEventOutboxMapper
{
    public static string GetEventName(IDomainEvent e)=>e switch
    {
        ContentItemCreatedDomainEvent=>"content.item.created",ContentItemUpdatedDomainEvent=>"content.item.updated",ContentItemSubmittedForReviewDomainEvent=>"content.item.review-requested",ContentItemPublishedDomainEvent=>"content.item.published",ContentItemScheduledDomainEvent=>"content.item.scheduled",ContentItemUnpublishedDomainEvent=>"content.item.unpublished",ContentItemArchivedDomainEvent=>"content.item.archived",ContentItemDeletedDomainEvent=>"content.item.deleted",ContentItemRevisionRestoredDomainEvent=>"content.item.revision-restored",
        TaxonomyTermCreatedDomainEvent=>"content.taxonomy.created",TaxonomyTermUpdatedDomainEvent=>"content.taxonomy.updated",TaxonomyTermDeletedDomainEvent=>"content.taxonomy.deleted",
        MediaReferenceCreatedDomainEvent=>"content.media.created",MediaReferenceDeletedDomainEvent=>"content.media.deleted",
        NavigationMenuChangedDomainEvent=>"content.menu.changed",SiteSettingChangedDomainEvent=>"content.setting.changed",
        MarketingLeadCreatedDomainEvent=>"content.marketing-lead.created",MarketingLeadUpdatedDomainEvent=>"content.marketing-lead.updated",MarketingLeadDeletedDomainEvent=>"content.marketing-lead.deleted",
        _=>$"content.{e.GetType().Name}"
    };
    public static string GetEventType(IDomainEvent e)=>e.GetType().FullName??e.GetType().Name;
}
