using System.ComponentModel;
using MediatR;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using Application.Checkout.Cart.Commands;
using System.Text.Json.Nodes;
using Server.Common.Mcp.Attributes;
using Domain.Sellers;
using CartItemInput = Application.Checkout.Cart.Commands.CartItemInput;

namespace Server.Checkout.McpTools;

public sealed class AddToCartTool
{
    private readonly IMediator _mediator;
    private readonly ISellerRepository _sellerRepository;

    public AddToCartTool(IMediator mediator, ISellerRepository sellerRepository)
    {
        _mediator = mediator;
        _sellerRepository = sellerRepository;
    }

    /// <summary>
    /// Adds one or more items to the shopping cart. Creates a new cart if cartId is not provided.
    /// Returns the updated cart state with cartId for session persistence.
    /// Use the returned cartId in subsequent cart operations to maintain the same cart session.
    /// </summary>
    [OpenAiToolMetadataAttribute(
        ToolName = "add_to_cart",
        Visibility = "public",
        InvokingMessage = "Agregando productos al carrito...",
        InvokedMessage = "Productos agregados al carrito.")]
    [Description("Adds items to the shopping cart. Creates a new cart if cartId is not provided. Returns cartId for session persistence.")]
    public async Task<CallToolResult> AddToCart(
        McpServer server,
        [Description("Array of items to add. Each item must include: productId (string, required), productName (string, required), productSku (string, required), quantity (int, required, must be > 0), unitPrice (decimal, required), imageUrl (string, optional), category (string, optional), brand (string, optional), metadata (object, optional).")] CartItemInput[] items,
        [Description("Existing cart identifier from a previous cart operation. Leave blank or null to create a new cart. Use the cartId returned from get_cart, add_to_cart, update_cart_item, or remove_from_cart to maintain the same cart session.")] string? cartId = null,
        [Description("Shop key (string, optional). Get this value by calling 'get_available_sellers' tool first. Used to identify the seller/store. Examples: 'mercury', 'coolbox', 'promart', 'plazavea', 'oechsle', 'wong', 'metrope'.")] string? shopKey = null,
        CancellationToken cancellationToken = default)
    {
        // Get seller information if shopKey is provided
        string? sellerName = null;
        if (!string.IsNullOrEmpty(shopKey))
        {
            var seller = await _sellerRepository.GetByShopKeyAsync(shopKey, cancellationToken);
            sellerName = seller?.Name;
        }

        // Convert CartItemInput[] to List<CartItemInput> and add shopKey/sellerName to metadata
        var itemsList = items.Select(item =>
        {
            var metadata = item.Metadata ?? new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(shopKey) && !metadata.ContainsKey("shopKey"))
            {
                metadata["shopKey"] = shopKey;
            }
            if (!string.IsNullOrEmpty(sellerName) && !metadata.ContainsKey("sellerName"))
            {
                metadata["sellerName"] = sellerName;
            }
            
            return new CartItemInput(
                ProductId: item.ProductId,
                ProductName: item.ProductName,
                ProductSku: item.ProductSku,
                Quantity: item.Quantity,
                UnitPrice: item.UnitPrice,
                ImageUrl: item.ImageUrl,
                Category: item.Category,
                Brand: item.Brand,
                Metadata: metadata
            );
        }).ToList();

        var command = new AddToCartCommand(cartId, itemsList);
        var response = await _mediator.Send(command, cancellationToken);

        // Build structured content
        var itemsArray = new JsonArray();
        foreach (var item in response.Cart.Items)
        {
            var itemObj = new JsonObject
            {
                ["productId"] = item.ProductId,
                ["productName"] = item.ProductName,
                ["productSku"] = item.ProductSku,
                ["quantity"] = item.Quantity,
                ["unitPrice"] = item.UnitPrice,
                ["totalPrice"] = item.TotalPrice,
                ["imageUrl"] = item.ImageUrl ?? string.Empty
            };
            
            if (!string.IsNullOrEmpty(item.Category))
            {
                itemObj["category"] = item.Category;
            }
            
            if (!string.IsNullOrEmpty(item.Brand))
            {
                itemObj["brand"] = item.Brand;
            }

            // Extract sellerName and shopKey from metadata
            if (item.Metadata != null)
            {
                if (item.Metadata.TryGetValue("sellerName", out var sellerNameValue))
                {
                    itemObj["sellerName"] = sellerNameValue;
                }
                if (item.Metadata.TryGetValue("shopKey", out var shopKeyValue))
                {
                    itemObj["shopKey"] = shopKeyValue;
                }
            }
            
            itemsArray.Add(itemObj);
        }

        var structuredContent = new JsonObject
        {
            ["cartId"] = response.CartId,
            ["items"] = itemsArray,
            ["subTotal"] = response.Cart.SubTotal,
            ["tax"] = response.Cart.Tax,
            ["shipping"] = response.Cart.Shipping,
            ["total"] = response.Cart.Total,
            ["totalItems"] = response.TotalItems
        };

        // Build metadata with widgetSessionId
        var securitySchemes = new JsonArray
        {
            new JsonObject { ["type"] = "noauth" }
        };

        var meta = new JsonObject
        {
            ["securitySchemes"] = securitySchemes,
            ["openai/visibility"] = "public",
            ["openai/toolInvocation/invoking"] = "Agregando productos al carrito...",
            ["openai/toolInvocation/invoked"] = "Productos agregados al carrito.",
            ["openai/widgetSessionId"] = response.CartId
        };

        var message = $"Carrito {response.CartId} ahora tiene {response.TotalItems} artículo(s).";

        return new CallToolResult
        {
            Content = [
                new TextContentBlock {
                    Text = message
                }
            ],
            StructuredContent = structuredContent,
            Meta = meta
        };
    }
}
