using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Nodes;
using MediatR;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using Application.Catalog.Queries;
using Server.Common.Mcp.Attributes;
using Server.Helpers;
using Domain.Sellers;

namespace Server.Catalog.McpTools;

// Note: [McpServerToolType] removed - tools are registered manually in Program.cs
public sealed class ProductComparisonTool
{
    private readonly IMediator _mediator;
    private readonly ISellerRepository _sellerRepository;

    public ProductComparisonTool(IMediator mediator, ISellerRepository sellerRepository)
    {
        _mediator = mediator;
        _sellerRepository = sellerRepository;
    }

    /// <summary>
    /// Compares multiple products side-by-side by their product IDs.
    /// Returns detailed product information including: id, name, description, price, SKU, category, brand, images, stock, features, and attributes.
    /// Use this tool when the user wants to compare specific products (e.g., "compare product X with product Y").
    /// The response includes comparison data that can be displayed in a comparison widget.
    /// IMPORTANT: Always call 'get_available_sellers' first to get the shopKey.
    /// </summary>
    // Note: Tools are registered manually in Program.cs - no [McpServerTool] needed
    [OpenAiToolMetadata(
        ToolName = "product_comparison",
        OutputTemplate = "ui://widget/product-comparison.html",
        WidgetAccessible = true,
        ResultCanProduceWidget = true,
        Visibility = "public",
        InvokingMessage = "Ejecutando...",
        InvokedMessage = "Completado.")]
    [Description("Compares multiple products by their IDs. Returns detailed product information for side-by-side comparison. ALWAYS call 'get_available_sellers' first to get shopKey.")]
    public async Task<CallToolResult> ProductComparison(
        McpServer server,
        [Description("Array of product IDs (Guid[], required). Must contain at least 2 product IDs to compare. Each productId should be a valid GUID string from the catalog. Get product IDs from catalog_list results.")] Guid[] productIds,
        [Description("Shop key (string, REQUIRED). Get this value by calling 'get_available_sellers' tool first. Examples: 'mercury', 'coolbox', 'promart', 'plazavea', 'oechsle', 'wong', 'metrope'.")] string shopKey,
        CancellationToken cancellationToken = default)
    {
        var query = new CompareProductsQuery(productIds, shopKey);
        var response = await _mediator.Send(query);

        // Get seller information
        var seller = await _sellerRepository.GetByShopKeyAsync(shopKey, cancellationToken);
        var sellerName = seller?.Name ?? string.Empty;

        var productsArray = new JsonArray();
        foreach (var p in response.Products)
        {
            var featuresArray = new JsonArray();
            foreach (var feature in p.Features ?? new List<string>())
            {
                featuresArray.Add(feature);
            }

            var imageUrlsArray = new JsonArray();
            // Use ImageUrls if available, otherwise fallback to ImageUrl
            var imageUrls = p.ImageUrls != null && p.ImageUrls.Count > 0 
                ? p.ImageUrls 
                : (!string.IsNullOrEmpty(p.ImageUrl) ? new List<string> { p.ImageUrl } : new List<string>());
            foreach (var imageUrl in imageUrls)
            {
                if (!string.IsNullOrEmpty(imageUrl))
                {
                    imageUrlsArray.Add(imageUrl);
                }
            }

            var productObj = new JsonObject
            {
                ["id"] = p.Id.ToString(),
                ["name"] = p.Name,
                ["description"] = HtmlSanitizer.StripHtml(p.Description),
                ["price"] = p.Price,
                ["sku"] = p.SKU,
                ["category"] = p.Category,
                ["brand"] = p.Brand ?? string.Empty,
                ["sellerName"] = sellerName,
                ["shopKey"] = shopKey,
                ["imageUrl"] = p.ImageUrl ?? string.Empty, // Keep for backward compatibility
                ["imageUrls"] = imageUrlsArray,
                ["stock"] = p.Stock,
                ["attributes"] = JsonSerializer.SerializeToNode(p.Attributes) ?? new JsonObject(),
                ["features"] = featuresArray
            };
            productsArray.Add(productObj);
        }

        var structuredContent = new JsonObject
        {
            ["products"] = productsArray,
            ["comparisonData"] = JsonSerializer.SerializeToNode(response.ComparisonData) ?? new JsonObject(),
            ["sellerName"] = sellerName,
            ["shopKey"] = shopKey,
            ["status"] = "completed" // "loading" | "completed" | "error"
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
                ["openai/outputTemplate"] = "ui://widget/product-comparison.html",
                ["openai/widgetAccessible"] = true,
                ["openai/resultCanProduceWidget"] = true,
                ["openai/visibility"] = "public",
                ["openai/toolInvocation/invoking"] = "Ejecutando...",
                ["openai/toolInvocation/invoked"] = "Completado."
            }
        };
    }
}
