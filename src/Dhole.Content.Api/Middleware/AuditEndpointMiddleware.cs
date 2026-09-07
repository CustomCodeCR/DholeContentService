using System.Text.Json;
using CustomCodeFramework.Messaging.Outbox;
using Dhole.Content.Persistence.Auditing;
using Dhole.Content.Persistence.DbContexts;

namespace Dhole.Content.Api.Middleware;

public sealed class AuditEndpointMiddleware(RequestDelegate next, ILogger<AuditEndpointMiddleware> logger)
{
    private const string SourceService = "DholeContentService";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private static readonly string[] IgnoredPathPrefixes = ["/swagger", "/health", "/metrics", "/favicon.ico"];
    private static readonly string[] EntityIdKeys = ["id", "contentId", "revisionId", "mediaId", "taxonomyId", "menuId", "settingId", "fileId"];

    public async Task InvokeAsync(HttpContext context)
    {
        await next(context);
        if (!ShouldAudit(context)) return;

        try
        {
            var dbContext = context.RequestServices.GetService<ServiceDbContext>();
            if (dbContext is null) return;

            var auditContext = AuditExecutionContextAccessor.Current;
            var correlationId = auditContext?.CorrelationId ?? Guid.NewGuid();
            var eventId = Guid.NewGuid();
            var requestPayload = new
            {
                Method = context.Request.Method,
                Path = context.Request.Path.Value,
                QueryString = context.Request.QueryString.Value,
                StatusCode = context.Response.StatusCode,
                Endpoint = context.GetEndpoint()?.DisplayName
            };
            var metadata = new
            {
                AuditLayer = "endpoint",
                RouteValues = context.Request.RouteValues.ToDictionary(x => x.Key, x => x.Value?.ToString()),
                Query = context.Request.Query.ToDictionary(x => x.Key, x => x.Value.ToString()),
                TraceIdentifier = context.TraceIdentifier
            };
            var payload = new
            {
                EventId = eventId,
                CorrelationId = correlationId,
                SourceService,
                EntityType = ResolveEntityType(context),
                EntityId = ResolveEntityId(context),
                Action = ResolveAction(context),
                EventType = ResolveEventType(context),
                UserId = auditContext?.UserId,
                UserName = auditContext?.UserName,
                IpAddress = auditContext?.IpAddress,
                UserAgent = auditContext?.UserAgent,
                OccurredAt = DateTime.UtcNow,
                BeforeJson = (string?)null,
                AfterJson = (string?)null,
                PayloadJson = JsonSerializer.Serialize(requestPayload, JsonOptions),
                Metadata = JsonSerializer.Serialize(metadata, JsonOptions),
                ErrorMessage = context.Response.StatusCode >= 400 ? $"HTTP {context.Response.StatusCode}" : null,
                StackTrace = (string?)null,
                Details = Array.Empty<object>()
            };

            dbContext.OutboxMessages.Add(new OutboxMessage
            {
                EventId = Guid.NewGuid(),
                EventType = "Dhole.AuditLogs.Contracts.AuditEvents.RegisterAuditEventRequest",
                EventName = "audit.event.registered",
                SourceService = SourceService,
                PayloadJson = JsonSerializer.Serialize(payload, JsonOptions),
                HeadersJson = null,
                CorrelationId = correlationId.ToString(),
                Status = OutboxMessageStatus.Pending,
                RetryCount = 0,
                ErrorMessage = null,
                CreatedAtUtc = DateTime.UtcNow
            });
            await dbContext.SaveChangesAsync(context.RequestAborted);
        }
        catch (OperationCanceledException) { }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to audit Content action {Method} {Path}.", context.Request.Method, context.Request.Path.Value);
        }
    }

    private static bool ShouldAudit(HttpContext context)
    {
        if (!context.Request.Path.StartsWithSegments("/api")) return false;
        var path = context.Request.Path.Value ?? string.Empty;
        return !IgnoredPathPrefixes.Any(x => path.StartsWith(x, StringComparison.OrdinalIgnoreCase));
    }

    private static string ResolveAction(HttpContext context)
    {
        if (context.Response.StatusCode == StatusCodes.Status401Unauthorized) return "unauthorized";
        if (context.Response.StatusCode == StatusCodes.Status403Forbidden) return "forbidden";
        if (context.Response.StatusCode >= 500) return "http_error";
        return context.Request.Method.ToUpperInvariant() switch
        {
            "GET" or "HEAD" => "viewed",
            "POST" => "created",
            "PUT" or "PATCH" => "updated",
            "DELETE" => "deleted",
            _ => "executed"
        };
    }

    private static string ResolveEventType(HttpContext context)
        => $"content.http.{ResolveEntityType(context)?.ToLowerInvariant() ?? "request"}.{ResolveAction(context)}";

    private static string? ResolveEntityType(HttpContext context)
    {
        var segments = context.Request.Path.Value?.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments is null || segments.Length == 0) return null;
        var apiIndex = Array.FindIndex(segments, x => x.Equals("api", StringComparison.OrdinalIgnoreCase));
        if (apiIndex < 0 || apiIndex + 2 >= segments.Length) return segments.LastOrDefault();
        var candidate = segments[apiIndex + 2];
        return candidate.ToLowerInvariant() switch
        {
            "content" => "Content",
            "pages" => "Page",
            "menus" => "NavigationMenu",
            "media" => "Media",
            "taxonomies" => "TaxonomyTerm",
            "settings" => "SiteSetting",
            "public" => "PublicContent",
            _ => candidate
        };
    }

    private static Guid? ResolveEntityId(HttpContext context)
    {
        foreach (var key in EntityIdKeys)
        {
            if (context.Request.RouteValues.TryGetValue(key, out var routeValue) && Guid.TryParse(routeValue?.ToString(), out var routeGuid)) return routeGuid;
            if (context.Request.Query.TryGetValue(key, out var queryValue) && Guid.TryParse(queryValue.ToString(), out var queryGuid)) return queryGuid;
        }
        var segments = context.Request.Path.Value?.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries) ?? [];
        foreach (var segment in segments) if (Guid.TryParse(segment, out var guid)) return guid;
        return null;
    }
}
