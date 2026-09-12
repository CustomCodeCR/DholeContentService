using CustomCodeFramework.Postgres.DependencyInjection;
using CustomCodeFramework.Postgres.EntityFramework.DependencyInjection;
using Dhole.Content.Application.Abstractions.Auditing;
using Dhole.Content.Application.Abstractions.Messaging;
using Dhole.Content.Application.Abstractions.Repositories;
using Dhole.Content.Persistence.Auditing;
using Dhole.Content.Persistence.DbContexts;
using Dhole.Content.Persistence.Messaging;
using Dhole.Content.Persistence.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dhole.Content.Persistence.DependencyInjection;

public static class PersistenceServiceCollectionExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCustomCodePostgres(configuration);
        services.AddCustomCodePostgresEntityFramework<ServiceDbContext>();
        services.AddScoped<IContentItemRepository, ContentItemRepository>();
        services.AddScoped<IContentRouteRepository, ContentRouteRepository>();
        services.AddScoped<ITaxonomyTermRepository, TaxonomyTermRepository>();
        services.AddScoped<IMediaReferenceRepository, MediaReferenceRepository>();
        services.AddScoped<IContentMediaRepository, ContentMediaRepository>();
        services.AddScoped<INavigationMenuRepository, NavigationMenuRepository>();
        services.AddScoped<ISiteSettingRepository, SiteSettingRepository>();
        services.AddScoped<ISiteRepository, SiteRepository>();
        services.AddScoped<IPlacementRepository, PlacementRepository>();
        services.AddScoped<IPlacementItemRepository, PlacementItemRepository>();
        services.AddScoped<ICollectionRepository, CollectionRepository>();
        services.AddScoped<ICollectionItemRepository, CollectionItemRepository>();
        services.AddScoped<IMarketingFormRepository, MarketingFormRepository>();
        services.AddScoped<IMarketingFormFieldRepository, MarketingFormFieldRepository>();
        services.AddScoped<IMarketingSubmissionRepository, MarketingSubmissionRepository>();
        services.AddScoped<ISeoRepository, SeoRepository>();
        services.AddScoped<IRedirectRepository, RedirectRepository>();
        services.AddScoped<IContentReviewRepository, ContentReviewRepository>();
        services.AddScoped<IIntegrationEventOutboxWriter, IntegrationEventOutboxWriter>();
        services.AddScoped<IContentAuditService, ContentAuditService>();
        return services;
    }
}
