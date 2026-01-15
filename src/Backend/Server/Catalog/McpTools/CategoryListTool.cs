using System.ComponentModel;
using MediatR;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using Application.Catalog.Queries;
using System.Text.Json.Nodes;
using Server.Common.Mcp.Attributes;

namespace Server.Catalog.McpTools;

// Note: [McpServerToolType] removed - tools are registered manually in Program.cs
public sealed class CategoryListTool
{
    private readonly IMediator _mediator;

    public CategoryListTool(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieves a list of all available product categories for a specific shop.
    /// Use this tool to discover available categories before filtering products in catalog_list.
    /// Returns an array of category names that can be used as the 'category' parameter in catalog_list.
    /// </summary>
    // Note: Tools are registered manually in Program.cs - no [McpServerTool] needed
    [OpenAiToolMetadata(
        ToolName = "get_available_categories",
        Visibility = "public",
        InvokingMessage = "Ejecutando...",
        InvokedMessage = "Completado.")]
    [Description("Retrieves all available product categories for a shop. Use the shopKey from 'get_available_sellers'. Returns category names for use in catalog_list filter.")]
    public async Task<CallToolResult> GetAvailableCategories(
        McpServer server,
        [Description("Shop key (string, REQUIRED). Get this value by calling 'get_available_sellers' tool first. Must match a shopKey from the sellers list.")] string shopKey,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAvailableCategoriesQuery(shopKey);
        var response = await _mediator.Send(query, cancellationToken);

        var categoriesArray = new JsonArray();
        foreach (var category in response.Categories)
        {
            categoriesArray.Add(category);
        }

        var structuredContent = new JsonObject
        {
            ["categories"] = categoriesArray,
            ["count"] = categoriesArray.Count
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
