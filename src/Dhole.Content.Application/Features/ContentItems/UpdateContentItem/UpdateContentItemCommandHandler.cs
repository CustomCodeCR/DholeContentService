using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;
using CustomCodeFramework.Persistence.Abstractions;
using Dhole.Content.Application.Abstractions.Auditing;
using Dhole.Content.Application.Abstractions.Cache;
using Dhole.Content.Application.Abstractions.Repositories;
using Dhole.Content.Application.Abstractions.Slugs;
using Dhole.Content.Application.Auditing;
using Dhole.Content.Domain.Shared;
namespace Dhole.Content.Application.ContentItems.UpdateContentItem;
public sealed class UpdateContentItemCommandHandler(IContentItemRepository contents,ISlugGenerator slugs,IContentAuditService audit,IContentCacheService cache,IUnitOfWork unitOfWork):ICommandHandler<UpdateContentItemCommand,Result>
{ public async Task<Result> HandleAsync(UpdateContentItemCommand c,CancellationToken ct=default){var item=await contents.GetByIdWithDetailsAsync(c.Id,ct);if(item is null||item.IsDeleted)return Result.Failure(ContentErrors.ContentNotFound);var before=ContentAuditSnapshots.From(item);var oldSlug=item.Slug;item.CreateRevision(c.UpdatedBy,"before-update");var slug=string.IsNullOrWhiteSpace(c.Slug)?item.Slug:slugs.Generate(c.Slug);if(await contents.ExistsBySlugAsync(item.SiteKey,slug,item.Id,ct))return Result.Failure(ContentErrors.ContentSlugAlreadyExists);item.Update(c.Title,slug,c.Excerpt,c.BlocksJson,c.RenderedHtml,c.FeaturedMediaId,c.SortOrder,c.IsFeatured,c.Locale,c.UpdatedBy);item.SetSeo(c.SeoTitle,c.SeoDescription,c.SeoKeywords,c.CanonicalUrl,c.Robots,c.OpenGraphMediaId,c.StructuredDataJson,c.UpdatedBy);item.ReplaceTaxonomies(c.TaxonomyTermIds);await audit.PublishAsync(new ContentAuditEvent(ContentAuditEventTypes.ContentUpdated,ContentAuditActions.Updated,ContentAuditEntityTypes.ContentItem,item.Id,c.UpdatedBy,Before:before,After:ContentAuditSnapshots.From(item)),ct);await unitOfWork.SaveChangesAsync(ct);await cache.RemoveContentAsync(item.SiteKey,oldSlug,ct);if(!string.Equals(oldSlug,item.Slug,StringComparison.OrdinalIgnoreCase))await cache.RemoveContentAsync(item.SiteKey,item.Slug,ct);return Result.Success();} }
