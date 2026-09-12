using System.Text.Json;
using CustomCodeFramework.Core.Domain.Entities;
using CustomCodeFramework.Messaging.Inbox;
using CustomCodeFramework.Messaging.Outbox;
using CustomCodeFramework.Postgres.EntityFramework.Configurations;
using CustomCodeFramework.Postgres.EntityFramework.DbContexts;
using Dhole.Content.Domain.Campaigns.Entities;
using Dhole.Content.Domain.Collections.Entities;
using Dhole.Content.Domain.Consents.Entities;
using Dhole.Content.Domain.ContentItems.Entities;
using Dhole.Content.Domain.Forms.Entities;
using Dhole.Content.Domain.Leads.Entities;
using Dhole.Content.Domain.Media.Entities;
using Dhole.Content.Domain.Meetings.Entities;
using Dhole.Content.Domain.Navigation.Entities;
using Dhole.Content.Domain.Placements.Entities;
using Dhole.Content.Domain.Redirects.Entities;
using Dhole.Content.Domain.Reviews.Entities;
using Dhole.Content.Domain.Routes.Entities;
using Dhole.Content.Domain.Settings.Entities;
using Dhole.Content.Domain.Sites.Entities;
using Dhole.Content.Domain.Submissions.Entities;
using Dhole.Content.Domain.Taxonomies.Entities;
using Dhole.Content.Persistence.Auditing;
using Dhole.Content.Persistence.Messaging;
using Microsoft.EntityFrameworkCore;

namespace Dhole.Content.Persistence.DbContexts;

public sealed class ServiceDbContext(DbContextOptions<ServiceDbContext> options) : AppDbContextBase(options)
{
    private const string SourceService = "DholeContentService";

    public DbSet<ContentItem> ContentItems => Set<ContentItem>();
    public DbSet<ContentRevision> ContentRevisions => Set<ContentRevision>();
    public DbSet<ContentTaxonomy> ContentTaxonomies => Set<ContentTaxonomy>();
    public DbSet<TaxonomyTerm> TaxonomyTerms => Set<TaxonomyTerm>();
    public DbSet<MediaReference> MediaReferences => Set<MediaReference>();
    public DbSet<ContentMedia> ContentMedia => Set<ContentMedia>();
    public DbSet<NavigationMenu> NavigationMenus => Set<NavigationMenu>();
    public DbSet<NavigationMenuItem> NavigationMenuItems => Set<NavigationMenuItem>();
    public DbSet<SiteSetting> SiteSettings => Set<SiteSetting>();
    public DbSet<Site> Sites => Set<Site>();
    public DbSet<ContentRoute> ContentRoutes => Set<ContentRoute>();
    public DbSet<Placement> Placements => Set<Placement>();
    public DbSet<PlacementItem> PlacementItems => Set<PlacementItem>();
    public DbSet<ContentCollection> Collections => Set<ContentCollection>();
    public DbSet<ContentCollectionItem> CollectionItems => Set<ContentCollectionItem>();
    public DbSet<MarketingForm> MarketingForms => Set<MarketingForm>();
    public DbSet<MarketingFormField> MarketingFormFields => Set<MarketingFormField>();
    public DbSet<MarketingSubmission> MarketingSubmissions => Set<MarketingSubmission>();
    public DbSet<MarketingLead> MarketingLeads => Set<MarketingLead>();
    public DbSet<MarketingConsent> MarketingConsents => Set<MarketingConsent>();
    public DbSet<MeetingType> MeetingTypes => Set<MeetingType>();
    public DbSet<MeetingRequest> MeetingRequests => Set<MeetingRequest>();
    public DbSet<MarketingCampaign> MarketingCampaigns => Set<MarketingCampaign>();
    public DbSet<ContentRedirect> Redirects => Set<ContentRedirect>();
    public DbSet<ContentReview> ContentReviews => Set<ContentReview>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        AddDomainEventsToOutbox();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        AddDomainEventsToOutbox();
        return base.SaveChanges();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("content");
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ServiceDbContext).Assembly);
        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
        modelBuilder.ApplyConfiguration(new InboxMessageConfiguration());
    }

    private void AddDomainEventsToOutbox()
    {
        var roots = ChangeTracker.Entries()
            .Select(x => x.Entity)
            .OfType<AggregateRoot<Guid>>()
            .Where(x => x.DomainEvents.Count > 0)
            .ToList();

        if (roots.Count == 0) return;

        var messages = new List<OutboxMessage>();
        foreach (var root in roots)
        {
            foreach (var domainEvent in root.DomainEvents)
            {
                var correlation = AuditExecutionContextAccessor.Current?.CorrelationId ?? Guid.NewGuid();
                messages.Add(new OutboxMessage
                {
                    EventId = domainEvent.EventId,
                    EventType = DomainEventOutboxMapper.GetEventType(domainEvent),
                    EventName = DomainEventOutboxMapper.GetEventName(domainEvent),
                    SourceService = SourceService,
                    PayloadJson = JsonSerializer.Serialize(domainEvent, domainEvent.GetType()),
                    HeadersJson = null,
                    CorrelationId = correlation.ToString(),
                    Status = OutboxMessageStatus.Pending,
                    RetryCount = 0,
                    ErrorMessage = null,
                    CreatedAtUtc = DateTime.UtcNow
                });
            }

            root.ClearDomainEvents();
        }

        OutboxMessages.AddRange(messages);
    }
}
