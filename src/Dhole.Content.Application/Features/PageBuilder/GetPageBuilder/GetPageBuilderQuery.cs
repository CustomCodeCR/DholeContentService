using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Queries;
using Dhole.Content.Contracts.PageBuilder;

namespace Dhole.Content.Application.PageBuilder.GetPageBuilder;

public sealed record GetPageBuilderQuery(Guid ContentId)
    : IQuery<Result<PageBuilderDocumentDto>>;
