using CustomCodeFramework.Cqrs.Commands;namespace Dhole.Content.Application.ContentItems.PublishDueContent;public sealed record PublishDueContentCommand(int BatchSize=100):ICommand<int>;
