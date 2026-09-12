using CustomCodeFramework.Api.DependencyInjection;
using CustomCodeFramework.Api.Swagger;
using CustomCodeFramework.Core.Abstractions;
using Dhole.Content.Api.Endpoints;
using Dhole.Content.Api.Grpc;
using Dhole.Content.Api.Middleware;
using Dhole.Content.Application.DependencyInjection;
using Dhole.Content.Infrastructure.DependencyInjection;
using Dhole.Content.Infrastructure.Time;
using Dhole.Content.Persistence.DbContexts;
using Dhole.Content.Persistence.DependencyInjection;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
const string CorsPolicyName = "DholeWebCors";

builder.Services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
builder.Services.AddCustomCodeApiWithSwagger(title: "Dhole Content Service", version: "v1");
builder.Services.AddGrpc();
builder.Services.AddCors(options =>
    options.AddPolicy(
        CorsPolicyName,
        policy =>
        {
            var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
            if (origins.Length > 0)
            {
                policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod();
            }
            else
            {
                policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
            }
        }
    )
);

builder.Services.AddApplication();
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);

var storageUrl = builder.Configuration["ServiceUrls:Storage"] ?? "http://localhost:5207/";
builder.Services.AddHttpClient("DholeStorage", client => client.BaseAddress = new Uri(storageUrl));

var app = builder.Build();

app.UseCustomCodeApi();
app.UseCors(CorsPolicyName);

if (app.Environment.IsDevelopment())
{
    app.UseCustomCodeSwagger();
}

app.MapGet(
        "/health",
        async (ServiceDbContext db, CancellationToken cancellationToken) =>
        {
            var healthy = false;
            try
            {
                healthy = await db.Database.CanConnectAsync(cancellationToken);
            }
            catch
            {
            }

            return Results.Json(
                new
                {
                    service = "DholeContentService",
                    status = healthy ? "Healthy" : "Unhealthy",
                    database = healthy ? "Connected" : "Unavailable",
                    timestamp = DateTimeOffset.UtcNow,
                },
                statusCode: healthy ? 200 : 503
            );
        }
    )
    .AllowAnonymous();

app.UseAuthentication();
app.UseMiddleware<AuditExecutionContextMiddleware>();
app.UseAuthorization();
app.UseMiddleware<AuditEndpointMiddleware>();

app.MapGrpcService<ContentQueryGrpcService>();
app.MapEditorEndpoints();
app.MapContentItemEndpoints();
app.MapContentRouteEndpoints();
app.MapPageBuilderEndpoints();
app.MapContentMediaEndpoints();
app.MapPlacementEndpoints();
app.MapCollectionEndpoints();
app.MapSeoEndpoints();
app.MapRedirectEndpoints();
app.MapContentReviewEndpoints();
app.MapTaxonomyEndpoints();
app.MapMediaEndpoints();
app.MapNavigationEndpoints();
app.MapSiteSettingEndpoints();
app.MapSiteEndpoints();
app.MapPublicContentEndpoints();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ServiceDbContext>();
    await db.Database.MigrateAsync();
}

await app.RunAsync();
