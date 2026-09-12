using System.Text.Json;
using System.Text.Json.Nodes;

namespace Dhole.Content.Domain.PageBuilder;

public static class PageBuilderDocument
{
    private static readonly string[] ForbiddenPropertyNames =
    [
        "script", "javascript", "customjs", "customscript", "vue", "component", "rawhtml", "unsafehtml"
    ];

    private static readonly string[] ForbiddenTextFragments =
    [
        "<script", "javascript:", "onerror=", "onload=", "onclick=", "onmouseover=", "v-html", "<iframe", "<object", "<embed"
    ];

    public static string NormalizeAndValidate(string? blocksJson)
    {
        var array = ParseArray(blocksJson);
        ValidateBlocks(array);
        return array.ToJsonString();
    }

    public static string ApplyOperation(
        string? blocksJson,
        string operation,
        string? blockId,
        string? blockType,
        int? targetIndex,
        bool? isVisible,
        string? dataJson)
    {
        var blocks = ParseArray(blocksJson);
        ValidateBlocks(blocks);
        var op = string.IsNullOrWhiteSpace(operation) ? string.Empty : operation.Trim().ToLowerInvariant();

        switch (op)
        {
            case "add":
            {
                if (!PageBuilderBlockTypes.IsSupported(blockType))
                    throw new ArgumentException("El tipo de bloque no es válido.", nameof(blockType));

                var block = new JsonObject
                {
                    ["id"] = Guid.NewGuid().ToString("N"),
                    ["type"] = PageBuilderBlockTypes.Normalize(blockType!),
                    ["isVisible"] = isVisible ?? true,
                    ["data"] = ParseData(dataJson)
                };
                Insert(blocks, block, targetIndex);
                break;
            }
            case "edit":
            {
                var block = FindBlock(blocks, blockId);
                if (!string.IsNullOrWhiteSpace(blockType))
                {
                    if (!PageBuilderBlockTypes.IsSupported(blockType))
                        throw new ArgumentException("El tipo de bloque no es válido.", nameof(blockType));
                    block["type"] = PageBuilderBlockTypes.Normalize(blockType);
                }
                if (dataJson is not null) block["data"] = ParseData(dataJson);
                if (isVisible.HasValue) block["isVisible"] = isVisible.Value;
                break;
            }
            case "delete":
            {
                var index = FindBlockIndex(blocks, blockId);
                blocks.RemoveAt(index);
                break;
            }
            case "duplicate":
            {
                var index = FindBlockIndex(blocks, blockId);
                var copy = (JsonObject)blocks[index]!.DeepClone();
                copy["id"] = Guid.NewGuid().ToString("N");
                blocks.Insert(index + 1, copy);
                break;
            }
            case "move":
            {
                if (!targetIndex.HasValue) throw new ArgumentException("TargetIndex es obligatorio para mover un bloque.", nameof(targetIndex));
                var index = FindBlockIndex(blocks, blockId);
                var node = blocks[index]!;
                blocks.RemoveAt(index);
                var destination = Math.Clamp(targetIndex.Value, 0, blocks.Count);
                blocks.Insert(destination, node);
                break;
            }
            case "hide":
                FindBlock(blocks, blockId)["isVisible"] = false;
                break;
            case "show":
                FindBlock(blocks, blockId)["isVisible"] = true;
                break;
            case "visibility":
                if (!isVisible.HasValue) throw new ArgumentException("IsVisible es obligatorio.", nameof(isVisible));
                FindBlock(blocks, blockId)["isVisible"] = isVisible.Value;
                break;
            default:
                throw new ArgumentException("Operación de Page Builder no válida.", nameof(operation));
        }

        ValidateBlocks(blocks);
        return blocks.ToJsonString();
    }

    private static JsonArray ParseArray(string? blocksJson)
    {
        if (string.IsNullOrWhiteSpace(blocksJson)) return new JsonArray();
        try
        {
            var node = JsonNode.Parse(blocksJson);
            return node as JsonArray ?? throw new ArgumentException("BlocksJson debe ser un arreglo JSON.", nameof(blocksJson));
        }
        catch (JsonException ex)
        {
            throw new ArgumentException("BlocksJson no contiene JSON válido.", nameof(blocksJson), ex);
        }
    }

    private static JsonObject ParseData(string? dataJson)
    {
        if (string.IsNullOrWhiteSpace(dataJson)) return new JsonObject();
        try
        {
            var node = JsonNode.Parse(dataJson);
            return node as JsonObject ?? throw new ArgumentException("DataJson debe ser un objeto JSON.", nameof(dataJson));
        }
        catch (JsonException ex)
        {
            throw new ArgumentException("DataJson no contiene JSON válido.", nameof(dataJson), ex);
        }
    }

    private static void ValidateBlocks(JsonArray blocks)
    {
        var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var node in blocks)
        {
            if (node is not JsonObject block) throw new ArgumentException("Cada bloque debe ser un objeto JSON.");
            var id = block["id"]?.GetValue<string>();
            var type = block["type"]?.GetValue<string>();
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Cada bloque requiere id.");
            if (!ids.Add(id)) throw new ArgumentException("Los ids de bloques deben ser únicos.");
            if (!PageBuilderBlockTypes.IsSupported(type)) throw new ArgumentException($"Tipo de bloque no soportado: {type}.");
            block["type"] = PageBuilderBlockTypes.Normalize(type!);
            block["isVisible"] ??= true;
            block["data"] ??= new JsonObject();
            if (block["data"] is not JsonObject) throw new ArgumentException("La propiedad data del bloque debe ser un objeto JSON.");
            ValidateSafeNode(block["data"]!, "data");
        }
    }

    private static void ValidateSafeNode(JsonNode node, string path)
    {
        if (node is JsonObject obj)
        {
            foreach (var pair in obj)
            {
                if (ForbiddenPropertyNames.Contains(pair.Key, StringComparer.OrdinalIgnoreCase))
                    throw new ArgumentException($"La propiedad '{pair.Key}' no está permitida en Page Builder ({path}).");
                if (pair.Value is not null) ValidateSafeNode(pair.Value, $"{path}.{pair.Key}");
            }
            return;
        }

        if (node is JsonArray array)
        {
            foreach (var child in array)
                if (child is not null) ValidateSafeNode(child, path);
            return;
        }

        if (node is JsonValue value && value.TryGetValue<string>(out var text))
        {
            foreach (var fragment in ForbiddenTextFragments)
                if (text.Contains(fragment, StringComparison.OrdinalIgnoreCase))
                    throw new ArgumentException($"Contenido no seguro detectado en Page Builder ({path}).");
        }
    }

    private static JsonObject FindBlock(JsonArray blocks, string? blockId)
        => (JsonObject)blocks[FindBlockIndex(blocks, blockId)]!;

    private static int FindBlockIndex(JsonArray blocks, string? blockId)
    {
        if (string.IsNullOrWhiteSpace(blockId)) throw new ArgumentException("BlockId es obligatorio.", nameof(blockId));
        for (var i = 0; i < blocks.Count; i++)
            if (blocks[i] is JsonObject block && string.Equals(block["id"]?.GetValue<string>(), blockId, StringComparison.OrdinalIgnoreCase))
                return i;
        throw new KeyNotFoundException("No se encontró el bloque solicitado.");
    }

    private static void Insert(JsonArray blocks, JsonObject block, int? targetIndex)
    {
        var index = targetIndex.HasValue ? Math.Clamp(targetIndex.Value, 0, blocks.Count) : blocks.Count;
        blocks.Insert(index, block);
    }
}
