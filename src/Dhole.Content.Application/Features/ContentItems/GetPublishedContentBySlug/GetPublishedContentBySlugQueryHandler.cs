using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Queries;
using Dhole.Content.Application.Abstractions.Cache;
using Dhole.Content.Application.Abstractions.Repositories;
using Dhole.Content.Application.Mappings;
using Dhole.Content.Contracts.ContentItems;
using Dhole.Content.Domain.Shared;
namespace Dhole.Content.Application.ContentItems.GetPublishedContentBySlug;
public sealed class GetPublishedContentBySlugQueryHandler(IContentItemRepository contents,IContentCacheService cache):IQueryHandler<GetPublishedContentBySlugQuery,Result<ContentItemDto>>
{ public async Task<Result<ContentItemDto>> HandleAsync(GetPublishedContentBySlugQuery q,CancellationToken ct=default){var site=string.IsNullOrWhiteSpace(q.SiteKey)?ContentConstants.DefaultSiteKey:q.SiteKey.Trim();var locale=string.IsNullOrWhiteSpace(q.Locale)?ContentConstants.DefaultLocale:q.Locale.Trim();var cached=await cache.GetPublishedContentAsync(site,q.Slug,locale,ct);if(cached is not null)return Result.Success(cached);var item=await contents.GetPublishedBySlugAsync(site,q.Slug,locale,ct);if(item is null)return Result.Failure<ContentItemDto>(ContentErrors.ContentNotFound);var dto=ContentMappings.ToDto(item);await cache.SetPublishedContentAsync(site,item.Slug,locale,dto,cancellationToken:ct);return Result.Success(dto);} }
