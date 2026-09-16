using System.Text.Json;
using CustomCodeFramework.Core.Pagination;
using CustomCodeFramework.Cqrs.Dispatching;
using Dhole.Content.Application.Abstractions.Cache;
using Dhole.Content.Application.Abstractions.Repositories;
using Dhole.Content.Application.Collections;
using Dhole.Content.Application.ContentItems.GetContentItems;
using Dhole.Content.Application.ContentItems.GetPublishedContentBySlug;
using Dhole.Content.Application.Forms;
using Dhole.Content.Application.Meetings;
using Dhole.Content.Application.Navigation.GetNavigationMenu;
using Dhole.Content.Application.Placements;
using Dhole.Content.Application.Routes.GetPublishedContentByRoute;
using Dhole.Content.Application.Settings.GetSiteSettings;
using Dhole.Content.Application.Submissions;
using Dhole.Content.Contracts.Meetings;
using Dhole.Content.Contracts.Placements;
using Dhole.Content.Contracts.Submissions;
using Dhole.Content.Domain.ContentItems.Enums;
using Dhole.Content.Domain.Forms;
using Dhole.Content.Domain.Submissions;

namespace Dhole.Content.Api.Endpoints;

public static class PublicContentEndpoints
{
    public static IEndpointRouteBuilder MapPublicContentEndpoints(this IEndpointRouteBuilder app)
    {
        var canonical = app.MapGroup("/api/public").WithTags("Public Content").AllowAnonymous();
        MapPublicReadEndpoints(canonical, includeLegacyContentRoutes: false);
        MapPublicSubmissionEndpoints(canonical);
        MapPublicMeetingEndpoints(canonical);

        var legacy = app.MapGroup("/api/content/public").WithTags("Public Content (Legacy)").AllowAnonymous();
        MapPublicReadEndpoints(legacy, includeLegacyContentRoutes: true);
        return app;
    }

    private static void MapPublicReadEndpoints(RouteGroupBuilder group, bool includeLegacyContentRoutes)
    {
        group.MapGet("/resolve", ResolveRouteAsync);
        group.MapGet("/routes/resolve", ResolveRouteAsync);
        group.MapGet("/media/{id:guid}/content", GetPublicMediaContentAsync);

        group.MapGet("/pages", async (int? pageNumber, int? pageSize, string? siteKey, string? search, string? locale, IQueryDispatcher dispatcher, CancellationToken ct) =>
            await GetPublishedListAsync(ContentType.Page, pageNumber, pageSize, siteKey, search, locale, dispatcher, ct));

        group.MapGet("/pages/{slug}", async (string slug, string? siteKey, string? locale, IQueryDispatcher dispatcher, HttpContext context, CancellationToken ct) =>
            EndpointResults.FromResult(await dispatcher.DispatchAsync(new GetPublishedContentBySlugQuery(siteKey ?? "main", slug, locale), ct), context));

        group.MapGet("/news", async (int? pageNumber, int? pageSize, string? siteKey, string? search, string? locale, IQueryDispatcher dispatcher, CancellationToken ct) =>
            await GetPublishedListAsync(ContentType.News, pageNumber, pageSize, siteKey, search, locale, dispatcher, ct));

        group.MapGet("/news/{slug}", async (string slug, string? siteKey, string? locale, IQueryDispatcher dispatcher, HttpContext context, CancellationToken ct) =>
            EndpointResults.FromResult(await dispatcher.DispatchAsync(new GetPublishedContentBySlugQuery(siteKey ?? "main", slug, locale), ct), context));

        group.MapGet("/menus/{location}", async (string location, string? siteKey, IQueryDispatcher dispatcher, HttpContext context, CancellationToken ct) =>
            EndpointResults.FromResult(await dispatcher.DispatchAsync(new GetNavigationMenuQuery(siteKey ?? "main", location), ct), context));

        group.MapGet("/settings", async (string? siteKey, IQueryDispatcher dispatcher, CancellationToken ct) =>
            EndpointResults.Ok(await dispatcher.DispatchAsync(new GetSiteSettingsQuery(siteKey ?? "main", true), ct)));

        group.MapGet("/placements", async (string? siteKey, IQueryDispatcher dispatcher, IContentCacheService cache, CancellationToken ct) =>
        {
            var resolvedSiteKey = siteKey ?? "main";
            var cached = await cache.GetPublicPlacementsAsync(resolvedSiteKey, ct);
            if (cached is not null) return EndpointResults.Ok(cached);
            var placements = await dispatcher.DispatchAsync(new GetPlacementsQuery(resolvedSiteKey), ct);
            var visible = placements.Where(item => item.IsActive).ToArray();
            await cache.SetPublicPlacementsAsync(resolvedSiteKey, visible, cancellationToken: ct);
            return EndpointResults.Ok(visible);
        });

        group.MapGet("/placements/{placementId:guid}/items", GetPublicPlacementItemsAsync);

        group.MapGet("/collections", async (string? siteKey, IQueryDispatcher dispatcher, IContentCacheService cache, CancellationToken ct) =>
        {
            var resolvedSiteKey = siteKey ?? "main";
            var cached = await cache.GetPublicCollectionsAsync(resolvedSiteKey, ct);
            if (cached is not null) return EndpointResults.Ok(cached);
            var collections = await dispatcher.DispatchAsync(new GetCollectionsQuery(resolvedSiteKey), ct);
            var visible = collections.Where(item => item.IsActive).ToArray();
            await cache.SetPublicCollectionsAsync(resolvedSiteKey, visible, cancellationToken: ct);
            return EndpointResults.Ok(visible);
        });

        group.MapGet("/collections/{collectionId:guid}/items", async (Guid collectionId, string? siteKey, IQueryDispatcher dispatcher, IContentCacheService cache, HttpContext context, CancellationToken ct) =>
        {
            var resolvedSiteKey = siteKey ?? "main";
            var cached = await cache.GetPublicCollectionItemsAsync(resolvedSiteKey, collectionId, ct);
            if (cached is not null) return EndpointResults.Ok(cached);
            var collections = await dispatcher.DispatchAsync(new GetCollectionsQuery(resolvedSiteKey), ct);
            if (!collections.Any(item => item.Id == collectionId && item.IsActive)) return Results.NotFound();
            var result = await dispatcher.DispatchAsync(new GetCollectionItemsQuery(collectionId), ct);
            if (!result.IsSuccess) return EndpointResults.FromResult(result, context);
            var visible = result.Value.Where(item => item.IsActive).ToArray();
            await cache.SetPublicCollectionItemsAsync(resolvedSiteKey, collectionId, visible, cancellationToken: ct);
            return EndpointResults.Ok(visible);
        });

        group.MapGet("/forms", async (string? siteKey, string? purpose, IQueryDispatcher dispatcher, CancellationToken ct) =>
            EndpointResults.Ok(await dispatcher.DispatchAsync(new GetMarketingFormsQuery(siteKey ?? "main", purpose, MarketingFormRules.StatusActive), ct)));

        group.MapGet("/forms/{formId:guid}/fields", async (Guid formId, string? siteKey, IQueryDispatcher dispatcher, HttpContext context, CancellationToken ct) =>
        {
            if (!await IsActiveFormAsync(formId, siteKey, dispatcher, ct)) return Results.NotFound();
            return EndpointResults.FromResult(await dispatcher.DispatchAsync(new GetMarketingFormFieldsQuery(formId), ct), context);
        });

        group.MapGet("/meetings/types", async (string? siteKey, IQueryDispatcher dispatcher, CancellationToken ct) =>
            EndpointResults.Ok(await dispatcher.DispatchAsync(new GetMeetingTypesQuery(siteKey ?? "main", true), ct)));

        if (includeLegacyContentRoutes)
        {
            group.MapGet("/content/{slug}", async (string slug, string? siteKey, string? locale, IQueryDispatcher dispatcher, HttpContext context, CancellationToken ct) =>
                EndpointResults.FromResult(await dispatcher.DispatchAsync(new GetPublishedContentBySlugQuery(siteKey ?? "main", slug, locale), ct), context));

            group.MapGet("/content", async (int? pageNumber, int? pageSize, string? siteKey, string? type, string? search, string? locale, IQueryDispatcher dispatcher, CancellationToken ct) =>
            {
                ContentType? parsedType = Enum.TryParse<ContentType>(type, true, out var typeValue) ? typeValue : null;
                var result = await dispatcher.DispatchAsync(new GetContentItemsQuery(PageRequest.Create(pageNumber ?? 1, pageSize ?? 20), siteKey ?? "main", parsedType, ContentStatus.Published, search, locale), ct);
                return EndpointResults.FromPaged(result);
            });
        }
    }

    private static void MapPublicSubmissionEndpoints(RouteGroupBuilder group)
    {
        group.MapPost("/forms/{formId:guid}/submissions", async (Guid formId, string? siteKey, SubmitMarketingFormRequest request, IQueryDispatcher queryDispatcher, ICommandDispatcher commandDispatcher, HttpContext context, CancellationToken ct) =>
        {
            if (!await IsActiveFormAsync(formId, siteKey, queryDispatcher, ct)) return Results.NotFound();
            var payloadJson = request.Payload.ValueKind == JsonValueKind.Undefined ? string.Empty : request.Payload.GetRawText();
            var correlationId = ResolveCorrelationId(context);
            var referrerUrl = string.IsNullOrWhiteSpace(request.ReferrerUrl) ? context.Request.Headers["Referer"].ToString() : request.ReferrerUrl;
            var ipHash = SubmissionRules.HashIp(context.Connection.RemoteIpAddress?.ToString());
            var userAgent = context.Request.Headers["User-Agent"].ToString();
            return EndpointResults.FromResult(await commandDispatcher.DispatchAsync(new SubmitMarketingFormCommand(formId, request.ContentId, request.CampaignId, request.SourceUrl, referrerUrl, request.UtmSource, request.UtmMedium, request.UtmCampaign, request.UtmContent, request.UtmTerm, payloadJson, ipHash, userAgent, correlationId, request.Consents), ct), context);
        });
    }

    private static void MapPublicMeetingEndpoints(RouteGroupBuilder group)
    {
        group.MapPost("/meetings/requests", async (PublicMeetingRequest request, IQueryDispatcher queryDispatcher, ICommandDispatcher commandDispatcher, HttpContext context, CancellationToken ct) =>
        {
            var activeTypes = await queryDispatcher.DispatchAsync(new GetMeetingTypesQuery(null, true), ct);
            if (!activeTypes.Any(item => item.Id == request.MeetingTypeId)) return Results.NotFound();
            return EndpointResults.FromResult(await commandDispatcher.DispatchAsync(new CreateMeetingRequestCommand(request.MeetingTypeId, null, null, request.RequestedStartUtc, request.RequestedEndUtc, request.TimeZone, request.Subject, request.Message, null), ct), context);
        });
    }

    private static async Task<IResult> GetPublicMediaContentAsync(
        Guid id,
        IMediaReferenceRepository mediaReferences,
        IHttpClientFactory clients,
        HttpContext context,
        CancellationToken ct)
    {
        var media = await mediaReferences.GetByIdAsync(id, ct);
        if (media is null || media.IsDeleted) return Results.NotFound();

        var client = clients.CreateClient("DholeStorage");
        using var response = await client.GetAsync($"api/v1/storage/files/{media.StorageFileId}/content", ct);
        if (!response.IsSuccessStatusCode)
            return Results.StatusCode((int)response.StatusCode);

        var bytes = await response.Content.ReadAsByteArrayAsync(ct);
        var contentType = response.Content.Headers.ContentType?.MediaType ?? media.ContentType;
        context.Response.Headers.CacheControl = "public, max-age=0, must-revalidate";
        context.Response.Headers["X-Content-Type-Options"] = "nosniff";
        return Results.File(bytes, contentType, enableRangeProcessing: true);
    }

    private static async Task<IResult> ResolveRouteAsync(string path, string? siteKey, string? locale, IQueryDispatcher dispatcher, HttpContext context, CancellationToken ct)
        => EndpointResults.FromResult(await dispatcher.DispatchAsync(new GetPublishedContentByRouteQuery(siteKey ?? "main", locale ?? "es-CR", path), ct), context);

    private static async Task<IResult> GetPublishedListAsync(ContentType type, int? pageNumber, int? pageSize, string? siteKey, string? search, string? locale, IQueryDispatcher dispatcher, CancellationToken ct)
    {
        var result = await dispatcher.DispatchAsync(new GetContentItemsQuery(PageRequest.Create(pageNumber ?? 1, pageSize ?? 20), siteKey ?? "main", type, ContentStatus.Published, search, locale), ct);
        return EndpointResults.FromPaged(result);
    }

    private static async Task<IResult> GetPublicPlacementItemsAsync(Guid placementId, string? siteKey, IQueryDispatcher dispatcher, IContentItemRepository contentRepository, IContentCacheService cache, HttpContext context, CancellationToken ct)
    {
        var resolvedSiteKey = siteKey ?? "main";
        var cached = await cache.GetPublicPlacementItemsAsync(resolvedSiteKey, placementId, ct);
        if (cached is not null) return EndpointResults.Ok(cached);
        var placements = await dispatcher.DispatchAsync(new GetPlacementsQuery(resolvedSiteKey), ct);
        if (!placements.Any(item => item.Id == placementId && item.IsActive)) return Results.NotFound();
        var result = await dispatcher.DispatchAsync(new GetPlacementItemsQuery(placementId), ct);
        if (!result.IsSuccess) return EndpointResults.FromResult(result, context);
        var now = DateTime.UtcNow;
        var visible = new List<PlacementItemDto>();
        foreach (var item in result.Value.Where(item => PublicContentPolicy.IsCurrentlyActive(item.IsActive, item.ValidFromUtc, item.ValidToUtc, now)))
        {
            var content = await contentRepository.GetByIdAsync(item.ContentId, ct);
            if (PublicContentPolicy.IsPublished(content)) visible.Add(item);
        }
        var expiration = ResolvePlacementExpiration(result.Value, now);
        await cache.SetPublicPlacementItemsAsync(resolvedSiteKey, placementId, visible, expiration, ct);
        return EndpointResults.Ok(visible);
    }

    private static TimeSpan? ResolvePlacementExpiration(IReadOnlyCollection<PlacementItemDto> items, DateTime now)
    {
        var boundary = items.SelectMany(item => new[] { item.ValidFromUtc, item.ValidToUtc }).Where(value => value.HasValue && value.Value > now).Select(value => value!.Value).OrderBy(value => value).FirstOrDefault();
        if (boundary == default) return null;
        var ttl = boundary - now;
        return ttl < TimeSpan.FromSeconds(5) ? TimeSpan.FromSeconds(5) : ttl;
    }

    private static async Task<bool> IsActiveFormAsync(Guid formId, string? siteKey, IQueryDispatcher dispatcher, CancellationToken ct)
    {
        var forms = await dispatcher.DispatchAsync(new GetMarketingFormsQuery(siteKey, null, MarketingFormRules.StatusActive), ct);
        return forms.Any(item => item.Id == formId);
    }

    private static Guid ResolveCorrelationId(HttpContext context)
    {
        var value = context.Request.Headers["X-Correlation-ID"].FirstOrDefault();
        return Guid.TryParse(value, out var correlationId) && correlationId != Guid.Empty ? correlationId : Guid.NewGuid();
    }

    public sealed record PublicMeetingRequest(Guid MeetingTypeId, DateTime RequestedStartUtc, DateTime RequestedEndUtc, string TimeZone, string? Subject, string? Message);
}
