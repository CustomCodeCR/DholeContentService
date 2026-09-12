using CustomCodeFramework.Cqrs.Queries;
using Dhole.Content.Contracts.Sites;

namespace Dhole.Content.Application.Sites.GetSites;

public sealed record GetSitesQuery() : IQuery<IReadOnlyCollection<SiteDto>>;
