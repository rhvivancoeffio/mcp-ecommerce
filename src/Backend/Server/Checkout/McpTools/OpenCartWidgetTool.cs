using System.ComponentModel;
using MediatR;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using Application.Checkout.Cart.Queries;
using System.Text.Json.Nodes;
using Server.Common.Mcp.Attributes;

namespace Server.Checkout.McpTools;

/// <summary>
/// Opens the shopping cart widget to display the current cart contents.
/// This tool is specifically designed to open and display the cart widget in the UI.
/// Use this when the user wants to view their cart or check what items they have added.
/// </summary>
public sealed class OpenCartWidgetTool
{
    private readonly IMediator _mediator;

    public OpenCartWidgetTool(IMediator mediator)
    {
        _mediator = mediator;
    }

    [OpenAiToolMetadataAttribute(
        ToolName = "open_cart_widget",
        OutputTemplate = "ui://widget/cart.html",
        WidgetAccessible = true,
        ResultCanProduceWidget = true,
        Visibility = "public",
        InvokingMessage = "Abriendo carrito...",
        InvokedMessage = "Carrito abierto.")]
    [Description("Opens the shopping cart widget to display the current cart contents. Use this when the user wants to view their cart. Requires cartId from previous cart operations.")]
    public async Task<CallToolResult> OpenCartWidget(
        McpServer server,
        [Description("Cart identifier (string, required). Use the cartId returned from add_to_cart, update_cart_item, remove_from_cart, or get_cart. This maintains the cart session across multiple operations.")] string cartId,
        CancellationToken cancellationToken = default)
    {
        var query = new GetCartQuery(cartId);
        var response = await _mediator.Send(query, cancellationToken);

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
            ["totalItems"] = response.TotalItems,
            ["status"] = "completed" // "loading" | "completed" | "error"
        };

        // Build metadata with widgetSessionId
        var securitySchemes = new JsonArray
        {
            new JsonObject { ["type"] = "noauth" }
        };

        var meta = new JsonObject
        {
            ["securitySchemes"] = securitySchemes,
            ["openai/outputTemplate"] = "ui://widget/cart.html",
            ["openai/widgetAccessible"] = true,
            ["openai/resultCanProduceWidget"] = true,
            ["openai/visibility"] = "public",
            ["openai/toolInvocation/invoking"] = "Abriendo carrito...",
            ["openai/toolInvocation/invoked"] = "Carrito abierto.",
            ["openai/widgetSessionId"] = response.CartId
        };

        var message = response.Cart.Items.Count == 0
            ? $"Carrito {response.CartId} está vacío."
            : $"Carrito {response.CartId} tiene {response.TotalItems} artículo(s).";

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
