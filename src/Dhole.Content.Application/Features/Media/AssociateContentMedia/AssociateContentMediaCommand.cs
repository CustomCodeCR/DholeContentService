using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;

namespace Dhole.Content.Application.Media.AssociateContentMedia;

public sealed record AssociateContentMediaCommand(
    Guid ContentId,
    Guid MediaReferenceId,
    string Role,
    int SortOrder,
    string? AltTextOverride,
    string? CaptionOverride,
    decimal? FocalX,
    decimal? FocalY,
    string? SettingsJson,
    Guid? ActorUserId) : ICommand<Result<Guid>>;
