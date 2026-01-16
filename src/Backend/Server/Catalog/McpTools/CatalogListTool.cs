using System.ComponentModel;
using MediatR;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using Application.Catalog.Queries;
using Application.Sellers.Queries;
using System.Text.Json;
using System.Text.Json.Nodes;
using Server.Common.Mcp.Attributes;
using Server.Helpers;

namespace Server.Catalog.McpTools;

// Note: [McpServerToolType] removed - tools are registered manually in Program.cs
public sealed class CatalogListTool
{
    private readonly IMediator _mediator;

    public CatalogListTool(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieves a paginated list of products from the catalog.
    /// Supports filtering by category and search term.
    /// Returns products with details including: id, name, description, price, SKU, category, brand, images, stock, features, and attributes.
    /// Use this tool to browse products, search for specific items, or filter by category.
    /// IMPORTANT: Always call 'get_available_sellers' first to get the shopKey, then use 'get_available_categories' or 'get_available_brands' to see filter options.
    /// </summary>
    // Note: Tools are registered manually in Program.cs - no [McpServerTool] needed
    [OpenAiToolMetadata(
        ToolName = "catalog_list",
        OutputTemplate = "ui://widget/catalog.html",
        WidgetAccessible = true,
        ResultCanProduceWidget = true,
        Visibility = "public",
        InvokingMessage = "Ejecutando...",
        InvokedMessage = "Completado.")]
    [Description("Retrieves a paginated list of products from the catalog. Supports filtering by category and search. ALWAYS call 'get_available_sellers' first to get shopKey.")]
    public async Task<CallToolResult> CatalogList(
        McpServer server,
        [Description("Shop key (string, REQUIRED). Get this value by calling 'get_available_sellers' tool first. Examples: 'mercury', 'coolbox', 'promart', 'plazavea', 'oechsle', 'wong', 'metrope'.")] string shopKey,
        [Description("Category name filter (string, optional). Use 'get_available_categories' tool with the same shopKey to see all available categories. Leave null to show all categories.")] string? category = null,
        [Description("Search term to filter products by name or description (string, optional). Performs text search across product names and descriptions. Leave null to show all products.")] string? searchTerm = null,
        [Description("Page number for pagination (int, optional, default: 1). Use this to navigate through multiple pages of results.")] int page = 1,
        [Description("Number of items per page (int, optional, default: 20). Maximum items returned in a single page. Adjust for performance or display preferences.")] int pageSize = 20,
        [Description("Cart identifier for session persistence (string, optional). Use cartId from previous cart operations (get_cart, add_to_cart) to maintain cart state in the widget. Leave null to start a new session.")] string? cartId = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetCatalogListQuery(shopKey, category, searchTerm, page, pageSize);
        var response = await _mediator.Send(query, cancellationToken);

        // Get seller information using query
        var sellerQuery = new GetSellerByShopKeyQuery(shopKey);
        var sellerResponse = await _mediator.Send(sellerQuery, cancellationToken);
        var sellerName = sellerResponse.Seller?.Name ?? string.Empty;

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
            ["totalCount"] = response.TotalCount,
            ["page"] = response.Page,
            ["pageSize"] = response.PageSize,
            ["sellerName"] = sellerName,
            ["shopKey"] = shopKey,
            ["status"] = "completed" // "loading" | "completed" | "error"
        };

        var securitySchemes = new JsonArray
        {
            new JsonObject { ["type"] = "noauth" }
        };

        // Build metadata with optional widgetSessionId
        var meta = new JsonObject
        {
            ["securitySchemes"] = securitySchemes,
            ["openai/outputTemplate"] = "ui://widget/catalog.html",
            ["openai/widgetAccessible"] = true,
            ["openai/resultCanProduceWidget"] = true,
            ["openai/visibility"] = "public",
            ["openai/toolInvocation/invoking"] = "Ejecutando...",
            ["openai/toolInvocation/invoked"] = "Completado."
        };

        // Add widgetSessionId if cartId is provided
        if (!string.IsNullOrEmpty(cartId))
        {
            meta["openai/widgetSessionId"] = cartId;
            structuredContent["cartId"] = cartId;
        }

        return new CallToolResult
        {
            Content = [
                new TextContentBlock {
                    Text = $"Datos disponibles en structuredContent"
                }
            ],
            StructuredContent = structuredContent,
            Meta = meta
        };
    }
}
