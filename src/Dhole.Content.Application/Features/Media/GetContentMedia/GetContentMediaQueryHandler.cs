using CustomCodeFramework.Cqrs.Queries;
using Dhole.Content.Application.Abstractions.Repositories;
using Dhole.Content.Contracts.Media;

namespace Dhole.Content.Application.Media.GetContentMedia;

public sealed class GetContentMediaQueryHandler(IContentMediaRepository repository)
    : IQueryHandler<GetContentMediaQuery, IReadOnlyCollection<ContentMediaDto>>
{
    public Task<IReadOnlyCollection<ContentMediaDto>> HandleAsync(
        GetContentMediaQuery query,
        CancellationToken cancellationToken = default)
        => repository.GetByContentAsync(query.ContentId, query.Role, cancellationToken);
}
