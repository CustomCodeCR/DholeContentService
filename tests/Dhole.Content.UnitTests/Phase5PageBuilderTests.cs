using System.Text.Json.Nodes;
using Dhole.Content.Domain.PageBuilder;
using Xunit;

namespace Dhole.Content.UnitTests;

public sealed class Phase5PageBuilderTests
{
    [Fact]
    public void BlockCatalog_ContainsAllPhase5Types()
    {
        string[] expected =
        [
            "Hero", "RichText", "Image", "Video", "Gallery", "CTA",
            "ServicesGrid", "NewsGrid", "FAQ", "Testimonials", "Logos",
            "Stats", "Team", "Banner", "Form", "MeetingForm"
        ];

        Assert.Equal(expected.Length, PageBuilderBlockTypes.All.Count);
        foreach (var type in expected)
            Assert.Contains(type, PageBuilderBlockTypes.All);
    }

    [Fact]
    public void Operations_AddEditDuplicateMoveHideShowAndDeleteBlocks()
    {
        var json = PageBuilderDocument.ApplyOperation(
            "[]",
            "add",
            null,
            "Hero",
            null,
            true,
            "{\"title\":\"Inicio\"}");

        var blocks = JsonNode.Parse(json)!.AsArray();
        Assert.Single(blocks);
        var firstId = blocks[0]!["id"]!.GetValue<string>();
        Assert.Equal("Hero", blocks[0]!["type"]!.GetValue<string>());

        json = PageBuilderDocument.ApplyOperation(
            json,
            "edit",
            firstId,
            null,
            null,
            null,
            "{\"title\":\"Bienvenidos\"}");
        blocks = JsonNode.Parse(json)!.AsArray();
        Assert.Equal("Bienvenidos", blocks[0]!["data"]!["title"]!.GetValue<string>());

        json = PageBuilderDocument.ApplyOperation(json, "duplicate", firstId, null, null, null, null);
        blocks = JsonNode.Parse(json)!.AsArray();
        Assert.Equal(2, blocks.Count);
        var duplicatedId = blocks[1]!["id"]!.GetValue<string>();
        Assert.NotEqual(firstId, duplicatedId);

        json = PageBuilderDocument.ApplyOperation(json, "move", duplicatedId, null, 0, null, null);
        blocks = JsonNode.Parse(json)!.AsArray();
        Assert.Equal(duplicatedId, blocks[0]!["id"]!.GetValue<string>());

        json = PageBuilderDocument.ApplyOperation(json, "hide", duplicatedId, null, null, null, null);
        blocks = JsonNode.Parse(json)!.AsArray();
        Assert.False(blocks[0]!["isVisible"]!.GetValue<bool>());

        json = PageBuilderDocument.ApplyOperation(json, "show", duplicatedId, null, null, null, null);
        blocks = JsonNode.Parse(json)!.AsArray();
        Assert.True(blocks[0]!["isVisible"]!.GetValue<bool>());

        json = PageBuilderDocument.ApplyOperation(json, "delete", duplicatedId, null, null, null, null);
        blocks = JsonNode.Parse(json)!.AsArray();
        Assert.Single(blocks);
        Assert.Equal(firstId, blocks[0]!["id"]!.GetValue<string>());
    }

    [Theory]
    [InlineData("{\"html\":\"<script>alert(1)</script>\"}")]
    [InlineData("{\"url\":\"javascript:alert(1)\"}")]
    [InlineData("{\"onclick\":\"doSomething()\"}")]
    [InlineData("{\"vue\":\"MyComponent\"}")]
    [InlineData("{\"rawHtml\":\"<b>unsafe</b>\"}")]
    public void Add_RejectsUnsafeOrExecutableContent(string dataJson)
    {
        Assert.Throws<ArgumentException>(() => PageBuilderDocument.ApplyOperation(
            "[]",
            "add",
            null,
            "RichText",
            null,
            true,
            dataJson));
    }

    [Fact]
    public void Normalize_RejectsUnsupportedBlockType()
    {
        const string json = "[{\"id\":\"block-1\",\"type\":\"CustomVue\",\"isVisible\":true,\"data\":{}}]";
        Assert.Throws<ArgumentException>(() => PageBuilderDocument.NormalizeAndValidate(json));
    }

    [Fact]
    public void Normalize_AddsVisibilityAndDataDefaults()
    {
        const string json = "[{\"id\":\"block-1\",\"type\":\"Hero\"}]";
        var normalized = PageBuilderDocument.NormalizeAndValidate(json);
        var block = JsonNode.Parse(normalized)!.AsArray()[0]!;

        Assert.True(block["isVisible"]!.GetValue<bool>());
        Assert.NotNull(block["data"]);
    }

    [Fact]
    public void Move_ClampsTargetIndexToDocumentBounds()
    {
        const string json = "[" +
            "{\"id\":\"a\",\"type\":\"Hero\",\"data\":{}}," +
            "{\"id\":\"b\",\"type\":\"Image\",\"data\":{}}]";

        var moved = PageBuilderDocument.ApplyOperation(json, "move", "a", null, 99, null, null);
        var blocks = JsonNode.Parse(moved)!.AsArray();

        Assert.Equal("b", blocks[0]!["id"]!.GetValue<string>());
        Assert.Equal("a", blocks[1]!["id"]!.GetValue<string>());
    }
}
