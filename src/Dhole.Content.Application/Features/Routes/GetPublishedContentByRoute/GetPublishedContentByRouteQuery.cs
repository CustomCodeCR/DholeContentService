using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Queries;
using Dhole.Content.Contracts.ContentItems;

namespace Dhole.Content.Application.Routes.GetPublishedContentByRoute;

public sealed record GetPublishedContentByRouteQuery(
    string SiteKey,
    string Locale,
    string Path
) : IQuery<Result<ContentItemDto>>;
