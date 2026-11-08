using System.Text.Json;
using Dhole.Content.Persistence.Messaging;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Dhole.Content.UnitTests;

public sealed class F6ContentRealtimeContractTests
{
    [Fact]
    public void All_four_release_flags_must_agree()
    {
        Assert.False(ContentRealtimeInvalidation.IsEnabled(null));
        var flags = new ConfigurationBuilder().AddInMemoryCollection(
            new Dictionary<string, string?>
            {
                ["Features:UniversalRealtimeEnabled"] = "true",
                ["Realtime:Outbox:Enabled"] = "true",
                ["Realtime:BatchApproved"] = "false",
                ["Realtime:ReleaseId"] = ContentRealtimeInvalidation.ReleaseId
            }).Build();
        Assert.False(ContentRealtimeInvalidation.IsEnabled(flags));
        flags["Realtime:BatchApproved"] = "true";
        Assert.True(ContentRealtimeInvalidation.IsEnabled(flags));
        flags["Realtime:ReleaseId"] = "foreign";
        Assert.False(ContentRealtimeInvalidation.IsEnabled(flags));
    }

    [Theory]
    [InlineData("item")]
    [InlineData("menu")]
    [InlineData("setting")]
    public void Content_updates_expose_only_resource_identity(string kind)
    {
        var id = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var json = ContentRealtimeInvalidation.Serialize(eventId, id,
            kind, ContentRealtimeInvalidation.NextVersionHint(),
            DateTimeOffset.UtcNow, "f6-content-ci");
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        Assert.Equal("content." + kind + ".changed.v1", root.GetProperty("eventType").GetString());
        Assert.Equal($"content:{kind}:{id:D}", root.GetProperty("audienceRef").GetString());
        Assert.Equal(kind, root.GetProperty("aggregateType").GetString());
        Assert.Equal(new[] { "changedFields" },
            root.GetProperty("payload").EnumerateObject().Select(x => x.Name));
        foreach (var field in new[] { "blocksJson", "seoTitle", "email", "token", "lead", "private" })
            Assert.DoesNotContain(field, json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Disallow_forged_resources_and_invalid_versions()
    {
        var id = Guid.NewGuid();
        Assert.Throws<ArgumentException>(() => ContentRealtimeInvalidation.EventType("marketing-lead"));
        Assert.Throws<ArgumentException>(() =>
            ContentRealtimeInvalidation.Serialize(id, Guid.Empty, "item", 1,
                DateTimeOffset.UtcNow, "test"));
        Assert.Throws<ArgumentException>(() =>
            ContentRealtimeInvalidation.Serialize(id, id, "item", 0,
                DateTimeOffset.UtcNow, "test"));
    }
}
