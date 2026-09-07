using CustomCodeFramework.Auth.DependencyInjection;
using CustomCodeFramework.Redis.DependencyInjection;
using Dhole.Content.Application.Abstractions;
using Dhole.Content.Infrastructure.Cache;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dhole.Content.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration, bool includeWebAuthentication = true)
    {
        if (includeWebAuthentication)
        {
            services.AddCustomCodeAuth(configuration);
            services.PostConfigure<AuthenticationOptions>(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            });
        }

        services.AddCustomCodeRedis(configuration);
        services.AddScoped<IContentCache, ContentCache>();
        return services;
    }
}
