namespace Dhole.Content.Application.Abstractions.Auditing;

public interface IContentAuditService
{
    Task PublishAsync(ContentAuditEvent auditEvent, CancellationToken cancellationToken = default);
}
