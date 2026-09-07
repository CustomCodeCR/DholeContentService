using Xunit;
using Dhole.Content.Domain.Content;

namespace Dhole.Content.UnitTests;

public sealed class ContentItemTests
{
    [Fact]
    public void Create_StartsAsDraftAndNormalizesSlug()
    {
        var item = ContentItem.Create(ContentType.News, "Nueva ruta", "Nueva Ruta Shanghai", "[]", null, Guid.NewGuid());
        Assert.Equal(ContentStatus.Draft, item.Status);
        Assert.Equal("nueva-ruta-shanghai", item.Slug);
    }

    [Fact]
    public void Publish_MakesContentPublic()
    {
        var item = ContentItem.Create(ContentType.Page, "Inicio", "inicio", "[]", null, Guid.NewGuid());
        var now = DateTime.UtcNow;
        item.Publish(now, Guid.NewGuid());
        Assert.Equal(ContentStatus.Published, item.Status);
        Assert.Equal(now, item.PublishedAtUtc);
    }

    [Fact]
    public void RestoreRevision_ReturnsToDraft()
    {
        var actor = Guid.NewGuid();
        var item = ContentItem.Create(ContentType.Post, "Uno", "uno", "[]", null, actor);
        var revision = item.Snapshot(actor, "test");
        item.Update("Dos", "dos", null, "[]", null, null, 0, false, null, actor);
        item.Restore(revision, actor);
        Assert.Equal("Uno", item.Title);
        Assert.Equal(ContentStatus.Draft, item.Status);
    }
}
