using Dhole.Content.Application.Abstractions;
using Dhole.Content.Contracts;
using Dhole.Content.Domain.Content;

namespace Dhole.Content.Application.Services;

public sealed class ContentApplicationService(
    IContentRepository repository,
    IContentCache cache,
    IContentEventPublisher eventPublisher)
{
    public async Task<PagedResult<ContentResponse>> BrowseAsync(
        string? search,
        string? type,
        string? status,
        string? siteKey,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var parsedType = ParseNullableEnum<ContentType>(type);
        var parsedStatus = ParseNullableEnum<ContentStatus>(status);
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var result = await repository.BrowseAsync(search, parsedType, parsedStatus, Site(siteKey), page, pageSize, cancellationToken);
        return new PagedResult<ContentResponse>(result.Items.Select(Map).ToArray(), page, pageSize, result.Total);
    }

    public async Task<ContentResponse?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var item = await repository.GetAsync(id, false, cancellationToken);
        return item is null || item.DeletedAtUtc is not null ? null : Map(item);
    }

    public async Task<ContentResponse> CreateAsync(ContentWriteRequest request, Guid? actor, CancellationToken cancellationToken)
    {
        var type = ParseEnum<ContentType>(request.Type);
        var siteKey = Site(request.SiteKey);
        if (await repository.SlugExistsAsync(ContentItem.NormalizeSlug(request.Slug), siteKey, null, cancellationToken))
            throw new InvalidOperationException("Ya existe contenido con ese slug.");

        var item = ContentItem.Create(type, request.Title, request.Slug, request.BlocksJson ?? "[]", request.AuthorUserId, actor, siteKey, request.Locale);
        item.Update(request.Title, request.Slug, request.Excerpt, request.BlocksJson ?? "[]", request.RenderedHtml, request.FeaturedMediaId, request.SortOrder, request.IsFeatured, request.Locale, actor);
        ApplySeo(item, request.Seo, actor);

        await repository.AddAsync(item, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        if (request.TaxonomyTermIds is { Count: > 0 })
        {
            await repository.ReplaceTaxonomiesAsync(item.Id, request.TaxonomyTermIds.Distinct().ToArray(), cancellationToken);
            await repository.SaveChangesAsync(cancellationToken);
        }
        return Map(item);
    }

    public async Task<ContentResponse> UpdateAsync(Guid id, ContentWriteRequest request, Guid? actor, CancellationToken cancellationToken)
    {
        var item = await RequiredAsync(id, cancellationToken);
        var normalizedSlug = ContentItem.NormalizeSlug(request.Slug);
        if (await repository.SlugExistsAsync(normalizedSlug, item.SiteKey, id, cancellationToken))
            throw new InvalidOperationException("Ya existe contenido con ese slug.");

        repository.AddRevision(await NextRevisionAsync(item, actor, "before-update", cancellationToken));
        item.Update(request.Title, request.Slug, request.Excerpt, request.BlocksJson ?? "[]", request.RenderedHtml, request.FeaturedMediaId, request.SortOrder, request.IsFeatured, request.Locale, actor);
        ApplySeo(item, request.Seo, actor);
        await repository.ReplaceTaxonomiesAsync(item.Id, request.TaxonomyTermIds?.Distinct().ToArray() ?? [], cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        await cache.RemoveContentAsync(item.SiteKey, item.Slug, cancellationToken);
        return Map(item);
    }

    public async Task SubmitForReviewAsync(Guid id, Guid? actor, CancellationToken cancellationToken)
    {
        var item = await RequiredAsync(id, cancellationToken);
        item.SubmitForReview(actor);
        await repository.SaveChangesAsync(cancellationToken);
    }

    public async Task PublishAsync(Guid id, Guid? actor, CancellationToken cancellationToken)
    {
        var item = await RequiredAsync(id, cancellationToken);
        repository.AddRevision(await NextRevisionAsync(item, actor, "before-publish", cancellationToken));
        item.Publish(DateTime.UtcNow, actor);
        await repository.SaveChangesAsync(cancellationToken);
        await cache.RemoveContentAsync(item.SiteKey, item.Slug, cancellationToken);
        await eventPublisher.QueuePublishedAsync(item, actor, cancellationToken);
    }

    public async Task ScheduleAsync(Guid id, DateTime scheduledAtUtc, Guid? actor, CancellationToken cancellationToken)
    {
        var item = await RequiredAsync(id, cancellationToken);
        item.Schedule(DateTime.SpecifyKind(scheduledAtUtc, DateTimeKind.Utc), actor);
        await repository.SaveChangesAsync(cancellationToken);
    }

    public async Task UnpublishAsync(Guid id, Guid? actor, CancellationToken cancellationToken)
    {
        var item = await RequiredAsync(id, cancellationToken);
        item.Unpublish(actor);
        await repository.SaveChangesAsync(cancellationToken);
        await cache.RemoveContentAsync(item.SiteKey, item.Slug, cancellationToken);
    }

    public async Task ArchiveAsync(Guid id, Guid? actor, CancellationToken cancellationToken)
    {
        var item = await RequiredAsync(id, cancellationToken);
        item.Archive(actor);
        await repository.SaveChangesAsync(cancellationToken);
        await cache.RemoveContentAsync(item.SiteKey, item.Slug, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, Guid? actor, CancellationToken cancellationToken)
    {
        var item = await RequiredAsync(id, cancellationToken);
        item.SoftDelete(actor);
        await repository.SaveChangesAsync(cancellationToken);
        await cache.RemoveContentAsync(item.SiteKey, item.Slug, cancellationToken);
    }

    public async Task<IReadOnlyCollection<RevisionResponse>> RevisionsAsync(Guid id, CancellationToken cancellationToken)
        => (await repository.GetRevisionsAsync(id, cancellationToken))
            .Select(x => new RevisionResponse(x.Id, x.RevisionNumber, x.Title, x.Slug, x.Reason, x.CreatedBy, x.CreatedAtUtc))
            .ToArray();

    public async Task RestoreRevisionAsync(Guid id, Guid revisionId, Guid? actor, CancellationToken cancellationToken)
    {
        var item = await RequiredAsync(id, cancellationToken);
        var revision = await repository.GetRevisionAsync(id, revisionId, cancellationToken)
            ?? throw new KeyNotFoundException("No existe la revisión.");
        repository.AddRevision(await NextRevisionAsync(item, actor, "before-restore", cancellationToken));
        item.Restore(revision, actor);
        await repository.SaveChangesAsync(cancellationToken);
        await cache.RemoveContentAsync(item.SiteKey, item.Slug, cancellationToken);
    }

    public async Task PublishDueAsync(DateTime utcNow, CancellationToken cancellationToken)
    {
        var due = await repository.GetScheduledDueAsync(utcNow, cancellationToken);
        foreach (var item in due)
        {
            item.Publish(utcNow, null);
            await cache.RemoveContentAsync(item.SiteKey, item.Slug, cancellationToken);
            await eventPublisher.QueuePublishedAsync(item, null, cancellationToken);
        }
        if (due.Count > 0)
            await repository.SaveChangesAsync(cancellationToken);
    }

    private async Task<ContentItem> RequiredAsync(Guid id, CancellationToken cancellationToken)
        => await repository.GetAsync(id, true, cancellationToken)
           ?? throw new KeyNotFoundException("No existe el contenido.");

    private async Task<ContentRevision> NextRevisionAsync(ContentItem item, Guid? actor, string reason, CancellationToken cancellationToken)
    {
        var current = await repository.GetRevisionsAsync(item.Id, cancellationToken);
        var revision = item.Snapshot(actor, reason);
        revision.RevisionNumber = current.Count == 0 ? 1 : current.Max(x => x.RevisionNumber) + 1;
        return revision;
    }

    private static void ApplySeo(ContentItem item, SeoWriteRequest? seo, Guid? actor)
    {
        if (seo is null) return;
        item.SetSeo(seo.Title, seo.Description, seo.Keywords, seo.CanonicalUrl, seo.Robots, seo.OpenGraphMediaId, seo.StructuredDataJson, actor);
    }

    public static ContentResponse Map(ContentItem x)
        => new(
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
