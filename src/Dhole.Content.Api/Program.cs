using CustomCodeFramework.Api.DependencyInjection;
using CustomCodeFramework.Api.Swagger;
using CustomCodeFramework.Core.Abstractions;
using Dhole.Content.Api.Endpoints;
using Dhole.Content.Api.Grpc;
using Dhole.Content.Api.Middleware;
using Dhole.Content.Api.Preview;
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
builder.Services.AddCors(options => options.AddPolicy(CorsPolicyName, policy =>
{
    var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
    if (origins.Length > 0) policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod();
    else policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
}));
var previewSigningSecret = builder.Configuration["Preview:SigningKey"] ?? builder.Configuration["Auth:Jwt:SecretKey"] ?? throw new InvalidOperationException("Preview:SigningKey or Auth:Jwt:SecretKey must be configured.");
builder.Services.AddSingleton(new ContentPreviewTokenService(previewSigningSecret));
builder.Services.AddApplication();
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);
var storageUrl = builder.Configuration["ServiceUrls:Storage"] ?? "http://localhost:5207/";
builder.Services.AddHttpClient("DholeStorage", client => client.BaseAddress = new Uri(storageUrl));
var app = builder.Build();
app.UseCustomCodeApi();
app.UseCors(CorsPolicyName);
if (app.Environment.IsDevelopment()) app.UseCustomCodeSwagger();
app.MapGet("/health", async (ServiceDbContext db, CancellationToken ct) =>
{
    var healthy = false; try { healthy = await db.Database.CanConnectAsync(ct); } catch { }
    return Results.Json(new { service = "DholeContentService", status = healthy ? "Healthy" : "Unhealthy", database = healthy ? "Connected" : "Unavailable", timestamp = DateTimeOffset.UtcNow }, statusCode: healthy ? 200 : 503);
}).AllowAnonymous();
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
app.MapMarketingFormEndpoints();
app.MapMarketingSubmissionEndpoints();
app.MapMarketingLeadEndpoints();
app.MapMarketingConsentEndpoints();
app.MapMeetingEndpoints();
app.MapMarketingCampaignEndpoints();
app.MapSeoEndpoints();
app.MapRedirectEndpoints();
app.MapContentReviewEndpoints();
app.MapTaxonomyEndpoints();
app.MapMediaEndpoints();
app.MapNavigationEndpoints();
app.MapSiteSettingEndpoints();
app.MapRealtimeAccessEndpoints();
app.MapSiteEndpoints();
app.MapPreviewEndpoints();
app.MapCmsAdminAliases();
app.MapPublicContentEndpoints();
app.MapPublicAnalyticsEndpoints();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ServiceDbContext>();
    await db.Database.MigrateAsync();
}
// Dhole API Manager: authenticated runtime inventory, not a public proxy.
app.MapGet("/api/internal/third-party-route-catalog", (HttpContext http) =>
{
    var allowed = http.User.Identity?.IsAuthenticated == true &&
        http.User.Claims.Where(c => c.Type is "scope" or "scp" or "scopes")
            .SelectMany(c => c.Value.Split([' ', ',', ';'], StringSplitOptions.RemoveEmptyEntries))
            .Contains("integrations.thirdparty.manage", StringComparer.OrdinalIgnoreCase);
    if (!allowed) return (IResult)Results.StatusCode(StatusCodes.Status403Forbidden);
    var routes = new List<object>();
    foreach (var source in ((Microsoft.AspNetCore.Routing.IEndpointRouteBuilder)app).DataSources)
        foreach (var endpoint in source.Endpoints.OfType<Microsoft.AspNetCore.Routing.RouteEndpoint>())
        {
            var methods = endpoint.Metadata.GetMetadata<Microsoft.AspNetCore.Routing.HttpMethodMetadata>()?.HttpMethods;
            if (methods is null || endpoint.RoutePattern.RawText is not { } path) continue;
            foreach (var method in methods)
                if (method is "GET" or "POST" or "PUT" or "PATCH" or "DELETE")
                    routes.Add(new { method, path = "/" + path.TrimStart('/') });
        }
    return (IResult)Results.Ok(routes);
}).RequireAuthorization().ExcludeFromDescription();

await app.RunAsync();
