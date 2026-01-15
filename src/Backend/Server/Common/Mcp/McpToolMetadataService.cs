using System.Reflection;
using System.Text.Json.Nodes;
using Server.Common.Mcp.Attributes;

namespace Server.Common.Mcp;

/// <summary>
/// Servicio para construir metadata de OpenAI para MCP Tools
/// </summary>
public class McpToolMetadataService
{
    private static readonly Dictionary<string, JsonObject> ToolMetadataCache = new();
    private static bool _cacheInitialized = false;
    private static readonly object _lockObject = new();

    public McpToolMetadataService()
    {
        InitializeCacheIfNeeded();
    }

    /// <summary>
    /// Obtiene la metadata para un tool específico
    /// </summary>
    public JsonObject? GetMetadataForTool(string toolName)
    {
        InitializeCacheIfNeeded();
        return ToolMetadataCache.TryGetValue(toolName, out var metadata) ? metadata : null;
    }

    /// <summary>
    /// Agrega _meta a un tool si tiene metadata disponible
    /// </summary>
    public void AddMetadataToTool(JsonObject toolObject, string toolName)
    {
        var metadata = GetMetadataForTool(toolName);
        if (metadata != null)
        {
            // Clonar el objeto metadata para evitar referencias compartidas y problemas de parent
            var metadataJson = metadata.ToJsonString();
            var clonedMetadata = JsonNode.Parse(metadataJson);
            toolObject["_meta"] = clonedMetadata;
        }
    }
    
    /// <summary>
    /// Crea una copia clonada de la metadata para evitar problemas de parent
    /// </summary>
    public JsonObject? GetClonedMetadataForTool(string toolName)
    {
        var metadata = GetMetadataForTool(toolName);
        if (metadata != null)
        {
            // Clonar el objeto metadata para evitar referencias compartidas
            var metadataJson = metadata.ToJsonString();
            return JsonNode.Parse(metadataJson) as JsonObject;
        }
        return null;
    }

    private void InitializeCacheIfNeeded()
    {
        if (_cacheInitialized)
            return;

        lock (_lockObject)
        {
            if (_cacheInitialized)
                return;

            // Buscar todos los métodos con atributo OpenAiToolMetadataAttribute
            // Note: Tools are registered manually - we only use ToolName from OpenAiToolMetadataAttribute
            // Search in both Catalog and Checkout assemblies
            var catalogAssembly = typeof(Server.Catalog.McpTools.CatalogListTool).Assembly;
            var checkoutAssembly = typeof(Server.Checkout.McpTools.AddToCartTool).Assembly;
            
            var toolMethods = new List<MethodInfo>();
            toolMethods.AddRange(catalogAssembly.GetTypes()
                .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Instance))
                .Where(m => m.GetCustomAttribute<OpenAiToolMetadataAttribute>() != null));
            
            toolMethods.AddRange(checkoutAssembly.GetTypes()
                .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Instance))
                .Where(m => m.GetCustomAttribute<OpenAiToolMetadataAttribute>() != null));

            foreach (var method in toolMethods)
            {
                var metadataAttr = method.GetCustomAttribute<OpenAiToolMetadataAttribute>();

                // Use ToolName from OpenAiToolMetadataAttribute (required)
                if (metadataAttr != null && !string.IsNullOrEmpty(metadataAttr.ToolName))
                {
                    var metadata = BuildMetadata(metadataAttr);
                    ToolMetadataCache[metadataAttr.ToolName] = metadata;
                }
            }

            _cacheInitialized = true;
        }
    }

    private JsonObject BuildMetadata(OpenAiToolMetadataAttribute? attr)
    {
        var securitySchemes = new JsonArray
        {
            new JsonObject { ["type"] = "noauth" }
        };

        var meta = new JsonObject
        {
            ["securitySchemes"] = securitySchemes,
            ["openai/visibility"] = attr?.Visibility ?? "public",
            ["openai/toolInvocation/invoking"] = attr?.InvokingMessage ?? "Ejecutando...",
            ["openai/toolInvocation/invoked"] = attr?.InvokedMessage ?? "Completado."
        };

        // Agregar campos específicos de widgets si están configurados
        if (!string.IsNullOrEmpty(attr?.OutputTemplate))
        {
            meta["openai/outputTemplate"] = attr.OutputTemplate;
            meta["openai/widgetAccessible"] = attr.WidgetAccessible;
            meta["openai/resultCanProduceWidget"] = attr.ResultCanProduceWidget;
        }

        return meta;
    }
}
