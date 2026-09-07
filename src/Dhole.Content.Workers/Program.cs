using Dhole.Content.Application.DependencyInjection;
using Dhole.Content.Infrastructure.DependencyInjection;
using Dhole.Content.Persistence.DependencyInjection;
using Dhole.Content.Workers;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddApplication();
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration, includeWebAuthentication: false);
builder.Services.AddHostedService<ScheduledPublishingWorker>();

await builder.Build().RunAsync();
