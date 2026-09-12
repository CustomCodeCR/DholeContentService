using CustomCodeFramework.Cqrs.Queries;
using Dhole.Content.Contracts.Media;

namespace Dhole.Content.Application.Media.GetContentMedia;

public sealed record GetContentMediaQuery(Guid ContentId, string? Role = null)
    : IQuery<IReadOnlyCollection<ContentMediaDto>>;
