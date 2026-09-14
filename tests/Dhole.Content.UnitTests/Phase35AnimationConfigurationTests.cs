using System.Text.Json.Nodes;
using Dhole.Content.Domain.PageBuilder;
using Xunit;

namespace Dhole.Content.UnitTests;

public sealed class Phase35AnimationConfigurationTests
{
    [Fact]
    public void Normalize_AllowsOptionalAnimationAndFillsCentralDefaults()
    {
        const string json = "[{\"id\":\"hero-1\",\"type\":\"Hero\",\"data\":{},\"animation\":{\"preset\":\"fade-up\"}}]";
        var normalized = PageBuilderDocument.NormalizeAndValidate(json);
        var animation = JsonNode.Parse(normalized)!.AsArray()[0]!["animation"]!.AsObject();

        Assert.Equal("fade-up", animation["preset"]!.GetValue<string>());
        Assert.Equal(600, animation["duration"]!.GetValue<int>());
        Assert.Equal(0, animation["delay"]!.GetValue<int>());
        Assert.Equal("standard", animation["easing"]!.GetValue<string>());
        Assert.Equal(100, animation["stagger"]!.GetValue<int>());
        Assert.Equal("scroll", animation["trigger"]!.GetValue<string>());
        Assert.True(animation["once"]!.GetValue<bool>());
        Assert.Equal(32, animation["distance"]!.GetValue<int>());
    }

    [Fact]
    public void Normalize_LeavesAnimationOptionalForBackwardCompatibility()
    {
        const string json = "[{\"id\":\"hero-1\",\"type\":\"Hero\",\"data\":{}}]";
        var normalized = PageBuilderDocument.NormalizeAndValidate(json);
        var block = JsonNode.Parse(normalized)!.AsArray()[0]!;
        Assert.Null(block["animation"]);
    }

    [Fact]
    public void Edit_CanPersistAnimationWithoutReplacingBlockData()
    {
        const string json = "[{\"id\":\"hero-1\",\"type\":\"Hero\",\"data\":{\"title\":\"Inicio\"}}]";
        var updated = PageBuilderDocument.ApplyOperation(
            json, "edit", "hero-1", null, null, null, null,
            "{\"preset\":\"zoom-in\",\"duration\":900,\"delay\":120,\"easing\":\"decelerate\",\"stagger\":80,\"trigger\":\"load\",\"once\":false,\"distance\":24}");
        var block = JsonNode.Parse(updated)!.AsArray()[0]!;

        Assert.Equal("Inicio", block["data"]!["title"]!.GetValue<string>());
        Assert.Equal("zoom-in", block["animation"]!["preset"]!.GetValue<string>());
        Assert.Equal(900, block["animation"]!["duration"]!.GetValue<int>());
        Assert.False(block["animation"]!["once"]!.GetValue<bool>());
    }

    [Theory]
    [InlineData("{\"preset\":\"spin\"}")]
    [InlineData("{\"easing\":\"elastic\"}")]
    [InlineData("{\"trigger\":\"hover\"}")]
    [InlineData("{\"duration\":3001}")]
    [InlineData("{\"delay\":-1}")]
    [InlineData("{\"stagger\":1001}")]
    [InlineData("{\"distance\":161}")]
    [InlineData("{\"once\":\"yes\"}")]
    [InlineData("{\"customJs\":\"alert(1)\"}")]
    public void Normalize_RejectsAnimationValuesOutsideThePhase34Contract(string animationJson)
    {
        var json = $"[{{\"id\":\"hero-1\",\"type\":\"Hero\",\"data\":{{}},\"animation\":{animationJson}}}]";
        Assert.Throws<ArgumentException>(() => PageBuilderDocument.NormalizeAndValidate(json));
    }

    [Fact]
    public void Duplicate_PreservesValidatedAnimationConfiguration()
    {
        const string json = "[{\"id\":\"hero-1\",\"type\":\"Hero\",\"data\":{},\"animation\":{\"preset\":\"fade\"}}]";
        var duplicated = PageBuilderDocument.ApplyOperation(json, "duplicate", "hero-1", null, null, null, null);
        var blocks = JsonNode.Parse(duplicated)!.AsArray();
        Assert.Equal("fade", blocks[1]!["animation"]!["preset"]!.GetValue<string>());
    }
}
