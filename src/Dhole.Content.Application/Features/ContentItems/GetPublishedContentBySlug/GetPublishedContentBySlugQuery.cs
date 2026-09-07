using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Queries;
using Dhole.Content.Contracts.ContentItems;
namespace Dhole.Content.Application.ContentItems.GetPublishedContentBySlug;
public sealed record GetPublishedContentBySlugQuery(string SiteKey,string Slug,string? Locale) : IQuery<Result<ContentItemDto>>;
