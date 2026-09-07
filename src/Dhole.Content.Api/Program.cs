using CustomCodeFramework.Api.DependencyInjection;
using CustomCodeFramework.Api.Swagger;
using Dhole.Content.Api.Endpoints;
using Dhole.Content.Application.DependencyInjection;
using Dhole.Content.Infrastructure.DependencyInjection;
using Dhole.Content.Persistence.DbContexts;
using Dhole.Content.Persistence.DependencyInjection;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

const string CorsPolicyName = "DholeContentCors";
var httpPort = int.TryParse(builder.Configuration["Http:Port"], out var configuredPort) && configuredPort > 0 ? configuredPort : 5210;

builder.WebHost.UseUrls($"http://0.0.0.0:{httpPort}");

builder.Services.AddCustomCodeApiWithSwagger(title: "Dhole Content Service", version: "v1");
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicyName, policy =>
    {
        var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
        if (origins is { Length: > 0 })
            policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod();
        else
            policy.WithOrigins(
                "https://dhole.customcodecr.com",
                "https://sistema.logisticacastrofallas.com",
                "https://logisticacastrofallas.com",
                "https://www.logisticacastrofallas.com",
                "http://localhost:5173",
                "http://127.0.0.1:5173")
                .AllowAnyHeader().AllowAnyMethod();
    });
});

builder.Services.AddApplication();
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHttpClient("DholeStorage", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:Storage:BaseUrl"] ?? "http://dhole-storage:5207/");
    client.Timeout = TimeSpan.FromMinutes(5);
});

var app = builder.Build();

app.UseCustomCodeApi();
app.UseCors(CorsPolicyName);

if (app.Environment.IsDevelopment())
    app.UseCustomCodeSwagger();

app.MapGet("/health", () => Results.Ok(new
{
    service = "DholeContentService",
    status = "Healthy",
    port = httpPort,
    timestamp = DateTimeOffset.UtcNow
})).AllowAnonymous();

app.UseAuthentication();
app.UseAuthorization();

app.MapPublicContentEndpoints();
app.MapContentEndpoints();
app.MapCmsAdministrationEndpoints();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ServiceDbContext>();
    await dbContext.Database.MigrateAsync();
}

await app.RunAsync();

public partial class Program;
