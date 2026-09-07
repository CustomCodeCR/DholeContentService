using CustomCodeFramework.Cqrs.DependencyInjection;
using CustomCodeFramework.Validation.DependencyInjection;
using Dhole.Content.Application.Abstractions.Slugs;
using Dhole.Content.Application.Slugs;
using Microsoft.Extensions.DependencyInjection;
namespace Dhole.Content.Application.DependencyInjection;
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddCustomCodeValidation(AssemblyReference.Assembly);
        services.AddCustomCodeCqrs(AssemblyReference.Assembly);
        services.AddCustomCodeCqrsBehaviors();
        services.AddScoped<ISlugGenerator,SlugGenerator>();
        return services;
    }
}
