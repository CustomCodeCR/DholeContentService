using Dhole.Content.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Dhole.Content.Application.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ContentApplicationService>();
        services.AddScoped<CmsAdministrationService>();
        return services;
    }
}
