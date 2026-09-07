using Dhole.Content.Application.Abstractions;

namespace Dhole.Content.Infrastructure.Cache;

// La primera versión invalida por contrato. Redis queda registrado por la infraestructura Dhole;
// FennecWeb puede cachear las lecturas públicas y escuchar content.published para invalidación inmediata.
public sealed class ContentCache : IContentCache
{
    public Task RemoveContentAsync(string siteKey, string slug, CancellationToken cancellationToken) => Task.CompletedTask;
    public Task RemoveMenuAsync(string siteKey, string location, CancellationToken cancellationToken) => Task.CompletedTask;
    public Task RemoveSettingsAsync(string siteKey, CancellationToken cancellationToken) => Task.CompletedTask;
}
