using System.ComponentModel;
using MediatR;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using Application.Catalog.Queries;
using System.Text.Json.Nodes;
using Server.Common.Mcp.Attributes;

namespace Server.Catalog.McpTools;

// Note: [McpServerToolType] removed - tools are registered manually in Program.cs
public sealed class BrandListTool
{
    private readonly IMediator _mediator;

    public BrandListTool(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieves a list of all available product brands for a specific shop.
    /// Use this tool to discover available brands before filtering or searching products.
    /// Returns an array of brand names that can be used for reference or filtering.
    /// </summary>
    // Note: Tools are registered manually in Program.cs - no [McpServerTool] needed
    [OpenAiToolMetadata(
        ToolName = "get_available_brands",
        Visibility = "public",
        InvokingMessage = "Ejecutando...",
        InvokedMessage = "Completado.")]
    [Description("Retrieves all available product brands for a shop. Use the shopKey from 'get_available_sellers'. Returns brand names for reference.")]
    public async Task<CallToolResult> GetAvailableBrands(
        McpServer server,
        [Description("Shop key (string, REQUIRED). Get this value by calling 'get_available_sellers' tool first. Must match a shopKey from the sellers list.")] string shopKey,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAvailableBrandsQuery(shopKey);
        var response = await _mediator.Send(query, cancellationToken);

        var brandsArray = new JsonArray();
        foreach (var brand in response.Brands)
        {
            brandsArray.Add(brand);
        }

        var structuredContent = new JsonObject
        {
            ["brands"] = brandsArray,
            ["count"] = brandsArray.Count
        };

        var securitySchemes = new JsonArray
        {
            new JsonObject { ["type"] = "noauth" }
        };

        return new CallToolResult
        {
            Content = [
                new TextContentBlock {
                    Text = $"Datos disponibles en structuredContent"
                }
            ],
            StructuredContent = structuredContent,
            Meta = new JsonObject
            {
                ["securitySchemes"] = securitySchemes,
                ["openai/visibility"] = "public",
                ["openai/toolInvocation/invoking"] = "Ejecutando...",
                ["openai/toolInvocation/invoked"] = "Completado."
            }
        };
    }
}
