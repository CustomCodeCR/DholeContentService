using CustomCodeFramework.Core.Results;
using CustomCodeFramework.Cqrs.Commands;

namespace Dhole.Content.Application.Media.UpdateContentMedia;

public sealed record UpdateContentMediaCommand(
    Guid ContentId,
    Guid Id,
    string Role,
    int SortOrder,
    string? AltTextOverride,
    string? CaptionOverride,
    decimal? FocalX,
    decimal? FocalY,
    string? SettingsJson,
    Guid? ActorUserId) : ICommand<Result>;
