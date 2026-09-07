using Dhole.Content.Application.Abstractions;
using Dhole.Content.Application.Abstractions.Auditing;
using Dhole.Content.Application.Auditing;
using Dhole.Content.Contracts;
using Dhole.Content.Domain.Content;

namespace Dhole.Content.Application.Services;

public sealed class ContentApplicationService(
    IContentRepository repository,
    IContentCache cache,
    IContentEventPublisher eventPublisher,
    IContentAuditService audit)
{
    public async Task<PagedResult<ContentResponse>> BrowseAsync(string? search, string? type, string? status, string? siteKey, int page, int pageSize, CancellationToken ct)
    {
        var parsedType = ParseNullableEnum<ContentType>(type);
        var parsedStatus = ParseNullableEnum<ContentStatus>(status);
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var result = await repository.BrowseAsync(search, parsedType, parsedStatus, Site(siteKey), page, pageSize, ct);
        return new PagedResult<ContentResponse>(result.Items.Select(Map).ToArray(), page, pageSize, result.Total);
    }

    public async Task<ContentResponse?> GetAsync(Guid id, CancellationToken ct)
    {
        var item = await repository.GetAsync(id, false, ct);
        return item is null || item.DeletedAtUtc is not null ? null : Map(item);
    }

    public async Task<ContentResponse> CreateAsync(ContentWriteRequest request, Guid? actor, CancellationToken ct)
    {
        var type = ParseEnum<ContentType>(request.Type);
        var siteKey = Site(request.SiteKey);
        if (await repository.SlugExistsAsync(ContentItem.NormalizeSlug(request.Slug), siteKey, null, ct))
            throw new InvalidOperationException("Ya existe contenido con ese slug.");

        var item = ContentItem.Create(type, request.Title, request.Slug, request.BlocksJson ?? "[]", request.AuthorUserId, actor, siteKey, request.Locale);
        item.Update(request.Title, request.Slug, request.Excerpt, request.BlocksJson ?? "[]", request.RenderedHtml, request.FeaturedMediaId, request.SortOrder, request.IsFeatured, request.Locale, actor);
        ApplySeo(item, request.Seo, actor);

        await repository.AddAsync(item, ct);
        await repository.ReplaceTaxonomiesAsync(item.Id, request.TaxonomyTermIds?.Distinct().ToArray() ?? [], ct);
        await audit.PublishAsync(new ContentAuditEvent(
            ContentAuditEventTypes.ContentCreated,
            ContentAuditActions.Created,
            ContentAuditEntityTypes.Content,
            item.Id,
            actor,
            After: ContentItemAuditSnapshot.From(item),
            Payload: new { item.Id, item.SiteKey, item.Slug, Type = item.Type.ToString() }), ct);
        await eventPublisher.QueueAsync(item, "content.created", actor, ct);
        await repository.SaveChangesAsync(ct);
        return Map(item);
    }

    public async Task<ContentResponse> UpdateAsync(Guid id, ContentWriteRequest request, Guid? actor, CancellationToken ct)
    {
        var item = await RequiredAsync(id, ct);
        var normalizedSlug = ContentItem.NormalizeSlug(request.Slug);
        if (await repository.SlugExistsAsync(normalizedSlug, item.SiteKey, id, ct))
            throw new InvalidOperationException("Ya existe contenido con ese slug.");

        var before = ContentItemAuditSnapshot.From(item);
        repository.AddRevision(await NextRevisionAsync(item, actor, "before-update", ct));
        item.Update(request.Title, request.Slug, request.Excerpt, request.BlocksJson ?? "[]", request.RenderedHtml, request.FeaturedMediaId, request.SortOrder, request.IsFeatured, request.Locale, actor);
        ApplySeo(item, request.Seo, actor);
        await repository.ReplaceTaxonomiesAsync(item.Id, request.TaxonomyTermIds?.Distinct().ToArray() ?? [], ct);
        await audit.PublishAsync(new ContentAuditEvent(
            ContentAuditEventTypes.ContentUpdated,
            ContentAuditActions.Updated,
            ContentAuditEntityTypes.Content,
            item.Id,
            actor,
            Before: before,
            After: ContentItemAuditSnapshot.From(item),
            Payload: new { item.Id, item.SiteKey, item.Slug }), ct);
        await eventPublisher.QueueAsync(item, "content.updated", actor, ct);
        await repository.SaveChangesAsync(ct);
        await cache.RemoveContentAsync(item.SiteKey, item.Slug, ct);
        return Map(item);
    }

    public Task SubmitForReviewAsync(Guid id, Guid? actor, CancellationToken ct)
        => ChangeStateAsync(id, actor, ContentAuditEventTypes.ContentSubmittedForReview, ContentAuditActions.SubmittedForReview, "content.submitted-for-review", (x, a) => x.SubmitForReview(a), ct);

    public async Task PublishAsync(Guid id, Guid? actor, CancellationToken ct)
    {
        var item = await RequiredAsync(id, ct);
        var before = ContentItemAuditSnapshot.From(item);
        repository.AddRevision(await NextRevisionAsync(item, actor, "before-publish", ct));
        item.Publish(DateTime.UtcNow, actor);
        await audit.PublishAsync(new ContentAuditEvent(
            ContentAuditEventTypes.ContentPublished,
            ContentAuditActions.Published,
            ContentAuditEntityTypes.Content,
            item.Id,
            actor,
            Before: before,
            After: ContentItemAuditSnapshot.From(item)), ct);
        await eventPublisher.QueueAsync(item, "content.published", actor, ct);
        await repository.SaveChangesAsync(ct);
        await cache.RemoveContentAsync(item.SiteKey, item.Slug, ct);
    }

    public async Task ScheduleAsync(Guid id, DateTime scheduledAtUtc, Guid? actor, CancellationToken ct)
    {
        var item = await RequiredAsync(id, ct);
        var before = ContentItemAuditSnapshot.From(item);
        item.Schedule(DateTime.SpecifyKind(scheduledAtUtc, DateTimeKind.Utc), actor);
        await audit.PublishAsync(new ContentAuditEvent(
            ContentAuditEventTypes.ContentScheduled,
            ContentAuditActions.Scheduled,
            ContentAuditEntityTypes.Content,
            item.Id,
            actor,
            Before: before,
            After: ContentItemAuditSnapshot.From(item),
            Payload: new { item.ScheduledAtUtc }), ct);
        await eventPublisher.QueueAsync(item, "content.scheduled", actor, ct);
        await repository.SaveChangesAsync(ct);
    }

    public Task UnpublishAsync(Guid id, Guid? actor, CancellationToken ct)
        => ChangeStateAsync(id, actor, ContentAuditEventTypes.ContentUnpublished, ContentAuditActions.Unpublished, "content.unpublished", (x, a) => x.Unpublish(a), ct, invalidateCache: true);

    public Task ArchiveAsync(Guid id, Guid? actor, CancellationToken ct)
        => ChangeStateAsync(id, actor, ContentAuditEventTypes.ContentArchived, ContentAuditActions.Archived, "content.archived", (x, a) => x.Archive(a), ct, invalidateCache: true);

    public Task DeleteAsync(Guid id, Guid? actor, CancellationToken ct)
        => ChangeStateAsync(id, actor, ContentAuditEventTypes.ContentDeleted, ContentAuditActions.Deleted, "content.deleted", (x, a) => x.SoftDelete(a), ct, invalidateCache: true);

    public async Task<IReadOnlyCollection<RevisionResponse>> RevisionsAsync(Guid id, CancellationToken ct)
        => (await repository.GetRevisionsAsync(id, ct)).Select(x => new RevisionResponse(x.Id, x.RevisionNumber, x.Title, x.Slug, x.Reason, x.CreatedBy, x.CreatedAtUtc)).ToArray();

    public async Task RestoreRevisionAsync(Guid id, Guid revisionId, Guid? actor, CancellationToken ct)
    {
        var item = await RequiredAsync(id, ct);
        var revision = await repository.GetRevisionAsync(id, revisionId, ct) ?? throw new KeyNotFoundException("No existe la revisión.");
        var before = ContentItemAuditSnapshot.From(item);
        repository.AddRevision(await NextRevisionAsync(item, actor, "before-restore", ct));
        item.Restore(revision, actor);
        await audit.PublishAsync(new ContentAuditEvent(
            ContentAuditEventTypes.ContentRevisionRestored,
            ContentAuditActions.Restored,
            ContentAuditEntityTypes.Content,
            item.Id,
            actor,
            Before: before,
            After: ContentItemAuditSnapshot.From(item),
            Payload: new { revisionId, revision.RevisionNumber }), ct);
        await eventPublisher.QueueAsync(item, "content.updated", actor, ct);
        await repository.SaveChangesAsync(ct);
        await cache.RemoveContentAsync(item.SiteKey, item.Slug, ct);
    }

    public async Task PublishDueAsync(DateTime utcNow, CancellationToken ct)
    {
        var due = await repository.GetScheduledDueAsync(utcNow, ct);
        foreach (var item in due)
        {
            var before = ContentItemAuditSnapshot.From(item);
            item.Publish(utcNow, null);
            await audit.PublishAsync(new ContentAuditEvent(
                ContentAuditEventTypes.ContentPublished,
                ContentAuditActions.Published,
                ContentAuditEntityTypes.Content,
                item.Id,
                ActorUserName: "Dhole.Content.Worker",
                Before: before,
                After: ContentItemAuditSnapshot.From(item),
                Metadata: new { BackgroundTask = "content.scheduled-publishing" }), ct);
            await eventPublisher.QueueAsync(item, "content.published", null, ct);
            await cache.RemoveContentAsync(item.SiteKey, item.Slug, ct);
        }
        if (due.Count > 0) await repository.SaveChangesAsync(ct);
    }

    private async Task ChangeStateAsync(Guid id, Guid? actor, string eventType, string action, string integrationEvent, Action<ContentItem, Guid?> mutate, CancellationToken ct, bool invalidateCache = false)
    {
        var item = await RequiredAsync(id, ct);
        var before = ContentItemAuditSnapshot.From(item);
        mutate(item, actor);
        await audit.PublishAsync(new ContentAuditEvent(eventType, action, ContentAuditEntityTypes.Content, item.Id, actor, Before: before, After: ContentItemAuditSnapshot.From(item)), ct);
        await eventPublisher.QueueAsync(item, integrationEvent, actor, ct);
        await repository.SaveChangesAsync(ct);
        if (invalidateCache) await cache.RemoveContentAsync(item.SiteKey, item.Slug, ct);
    }

    private async Task<ContentItem> RequiredAsync(Guid id, CancellationToken ct)
        => await repository.GetAsync(id, true, ct) ?? throw new KeyNotFoundException("No existe el contenido.");

    private async Task<ContentRevision> NextRevisionAsync(ContentItem item, Guid? actor, string reason, CancellationToken ct)
    {
        var current = await repository.GetRevisionsAsync(item.Id, ct);
        var revision = item.Snapshot(actor, reason);
        revision.RevisionNumber = current.Count == 0 ? 1 : current.Max(x => x.RevisionNumber) + 1;
        return revision;
    }

    private static void ApplySeo(ContentItem item, SeoWriteRequest? seo, Guid? actor)
    {
        if (seo is null) return;
        item.SetSeo(seo.Title, seo.Description, seo.Keywords, seo.CanonicalUrl, seo.Robots, seo.OpenGraphMediaId, seo.StructuredDataJson, actor);
    }

    public static ContentResponse Map(ContentItem x) => new(
        x.Id, x.Type.ToString(), x.Status.ToString(), x.Title, x.Slug, x.Excerpt,
        x.BlocksJson, x.RenderedHtml, x.FeaturedMediaId, x.AuthorUserId, x.Locale,
        x.SortOrder, x.IsFeatured,
        new SeoResponse(x.SeoTitle, x.SeoDescription, x.SeoKeywords, x.CanonicalUrl, x.Robots, x.OpenGraphMediaId, x.StructuredDataJson),
        x.ScheduledAtUtc, x.PublishedAtUtc, x.CreatedAtUtc, x.UpdatedAtUtc);

    private static T ParseEnum<T>(string value) where T : struct, Enum
        => Enum.TryParse<T>(value, true, out var parsed) ? parsed : throw new ArgumentException($"Valor {value} inválido para {typeof(T).Name}.");
    private static T? ParseNullableEnum<T>(string? value) where T : struct, Enum
        => string.IsNullOrWhiteSpace(value) ? null : ParseEnum<T>(value);
    private static string Site(string? value) => string.IsNullOrWhiteSpace(value) ? "main" : value.Trim();
}
