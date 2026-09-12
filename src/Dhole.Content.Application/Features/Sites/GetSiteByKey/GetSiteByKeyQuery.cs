using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Queries;
using Dhole.Content.Contracts.Sites;

namespace Dhole.Content.Application.Sites.GetSiteByKey;

public sealed record GetSiteByKeyQuery(string SiteKey) : IQuery<Result<SiteDto>>;
