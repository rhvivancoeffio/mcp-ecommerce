using System.ComponentModel;
using MediatR;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using Application.Sellers.Queries;
using System.Text.Json.Nodes;
using Server.Common.Mcp.Attributes;

namespace Server.Catalog.McpTools;

// Note: [McpServerToolType] removed - tools are registered manually in Program.cs
public sealed class SellerListTool
{
    private readonly IMediator _mediator;

    public SellerListTool(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieves a list of all available sellers (shops/stores) in the system.
    /// Each seller has: id, name, provider (Vtex, Shopify, etc.), and shopKey.
    /// IMPORTANT: This tool MUST be called FIRST before any catalog operations (catalog_list, get_available_categories, get_available_brands, product_comparison).
    /// Use the shopKey from the response as a required parameter in other catalog tools.
    /// </summary>
    // Note: Tools are registered manually in Program.cs - no [McpServerTool] needed
    [OpenAiToolMetadata(
        ToolName = "get_available_sellers",
        Visibility = "public",
        InvokingMessage = "Obteniendo tiendas disponibles...",
        InvokedMessage = "Tiendas obtenidas.")]
    [Description("Retrieves all available sellers (shops). MUST be called FIRST before any catalog operations. Use the shopKey from the response in other catalog tools.")]
    public async Task<CallToolResult> GetAvailableSellers(
        McpServer server,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAvailableSellersQuery();
        var response = await _mediator.Send(query, cancellationToken);

        var sellersArray = new JsonArray();
        foreach (var seller in response.Sellers)
        {
            var sellerObj = new JsonObject
            {
                ["id"] = seller.Id.ToString(),
                ["name"] = seller.Name,
                ["provider"] = seller.Provider.ToString(),
                ["shopKey"] = seller.ShopKey
            };
            sellersArray.Add(sellerObj);
        }

        var structuredContent = new JsonObject
        {
            ["sellers"] = sellersArray,
            ["count"] = sellersArray.Count
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
                ["openai/toolInvocation/invoking"] = "Obteniendo tiendas disponibles...",
                ["openai/toolInvocation/invoked"] = "Tiendas obtenidas."
            }
        };
    }
}
