using System.Reflection;
using System.Text.Json.Nodes;
using Server.Common.Mcp.Attributes;

namespace Server.Common.Mcp;

/// <summary>
/// Servicio para construir metadata de OpenAI para MCP Resources
/// </summary>
public class McpResourceMetadataService
{
    private static readonly Dictionary<string, JsonObject> ResourceMetadataCache = new();
    private static bool _cacheInitialized = false;
    private static readonly object _lockObject = new();

    public McpResourceMetadataService()
    {
        InitializeCacheIfNeeded();
    }

    /// <summary>
    /// Obtiene la metadata para un resource específico
    /// </summary>
    public JsonObject? GetMetadataForResource(string resourceUri)
    {
        InitializeCacheIfNeeded();
        return ResourceMetadataCache.TryGetValue(resourceUri, out var metadata) ? metadata : null;
    }

    /// <summary>
    /// Crea una copia clonada de la metadata para evitar problemas de parent
    /// </summary>
    public JsonObject? GetClonedMetadataForResource(string resourceUri)
    {
        var metadata = GetMetadataForResource(resourceUri);
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

            // Buscar todos los métodos con atributo OpenAiResourceMetadataAttribute
            // Search in Catalog assembly (resources are currently only in Catalog)
            var catalogAssembly = typeof(Server.Catalog.McpResources.CatalogHtmlResource).Assembly;
            
            var resourceMethods = catalogAssembly.GetTypes()
                .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Instance))
                .Where(m => m.GetCustomAttribute<OpenAiResourceMetadataAttribute>() != null)
                .ToList();

            foreach (var method in resourceMethods)
            {
                var metadataAttr = method.GetCustomAttribute<OpenAiResourceMetadataAttribute>();

                // Use Uri from OpenAiResourceMetadataAttribute (required)
                if (metadataAttr != null && !string.IsNullOrEmpty(metadataAttr.Uri))
                {
                    var metadata = BuildMetadata(metadataAttr);
                    ResourceMetadataCache[metadataAttr.Uri] = metadata;
                }
            }

            _cacheInitialized = true;
        }
    }

    private JsonObject BuildMetadata(OpenAiResourceMetadataAttribute? attr)
    {
        var meta = new JsonObject
        {
            ["openai/outputTemplate"] = attr?.Uri ?? string.Empty,
            ["openai/widgetAccessible"] = true, // Resources are always widget accessible
            ["openai/resultCanProduceWidget"] = true, // Resources always can produce widgets
            ["openai/toolInvocation/invoking"] = attr?.InvokingMessage ?? "Cargando...",
            ["openai/toolInvocation/invoked"] = attr?.InvokedMessage ?? "Recurso cargado."
        };

        return meta;
    }
}
