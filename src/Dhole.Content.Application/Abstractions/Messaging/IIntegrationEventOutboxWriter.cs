namespace Dhole.Content.Application.Abstractions.Messaging;
public interface IIntegrationEventOutboxWriter
{
    Task WriteAsync(string eventName,string eventType,object payload,string? correlationId=null,CancellationToken cancellationToken=default);
}
