using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Queries;
using Dhole.Content.Contracts.ContentItems;
namespace Dhole.Content.Application.ContentItems.GetContentItemById;
public sealed record GetContentItemByIdQuery(Guid Id) : IQuery<Result<ContentItemDto>>;
