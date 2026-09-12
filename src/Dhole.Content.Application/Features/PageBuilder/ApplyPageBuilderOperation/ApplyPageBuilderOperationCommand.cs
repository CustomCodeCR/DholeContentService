using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;
using Dhole.Content.Contracts.PageBuilder;

namespace Dhole.Content.Application.PageBuilder.ApplyPageBuilderOperation;

public sealed record ApplyPageBuilderOperationCommand(
    Guid ContentId,
    string Operation,
    string? BlockId,
    string? BlockType,
    int? TargetIndex,
    bool? IsVisible,
    string? DataJson,
    Guid? ActorUserId
) : ICommand<Result<PageBuilderDocumentDto>>;
