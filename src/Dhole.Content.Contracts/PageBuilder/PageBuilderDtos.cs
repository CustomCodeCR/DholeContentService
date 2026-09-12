namespace Dhole.Content.Contracts.PageBuilder;

public sealed record PageBuilderDocumentDto(
    Guid ContentId,
    string BlocksJson
);

public sealed record PageBuilderOperationRequest(
    string Operation,
    string? BlockId,
    string? BlockType,
    int? TargetIndex,
    bool? IsVisible,
    string? DataJson
);

public sealed record PageBuilderBlockTypeDto(
    string Type,
    string Label
);
