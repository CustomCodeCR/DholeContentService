using Dhole.Content.Domain.ContentItems.Entities;
using Dhole.Content.Domain.ContentItems.Enums;

namespace Dhole.Content.Api.Endpoints;

public static class PublicContentPolicy
{
    public static bool IsPublished(ContentItem? content)
        => content is not null && !content.IsDeleted && content.Status == ContentStatus.Published;

    public static bool IsCurrentlyActive(bool isActive, DateTime? validFromUtc, DateTime? validToUtc, DateTime utcNow)
        => isActive
           && (!validFromUtc.HasValue || validFromUtc.Value <= utcNow)
           && (!validToUtc.HasValue || validToUtc.Value > utcNow);
}
