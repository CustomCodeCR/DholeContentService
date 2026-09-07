using CustomCodeFramework.Cqrs.DependencyInjection;
using CustomCodeFramework.Validation.DependencyInjection;
using Dhole.Content.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Dhole.Content.Application.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddCustomCodeValidation(AssemblyReference.Assembly);
        services.AddCustomCodeCqrs(AssemblyReference.Assembly);
        services.AddCustomCodeCqrsBehaviors();
        services.AddScoped<ContentApplicationService>();
        services.AddScoped<CmsAdministrationService>();
        return services;
    }
}
