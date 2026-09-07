using CustomCodeFramework.Postgres.DependencyInjection;
using CustomCodeFramework.Postgres.EntityFramework.DependencyInjection;
using Dhole.Content.Application.Abstractions;
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
        services.AddScoped<IContentRepository, ContentRepository>();
        services.AddScoped<IContentEventPublisher, ContentEventPublisher>();
        return services;
    }
}
