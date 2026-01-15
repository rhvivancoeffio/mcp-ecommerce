using System.ComponentModel;
using MediatR;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using Application.Checkout.Cart.Commands;
using System.Text.Json.Nodes;
using Server.Common.Mcp.Attributes;

namespace Server.Checkout.McpTools;

public sealed class RemoveFromCartTool
{
    private readonly IMediator _mediator;

    public RemoveFromCartTool(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Removes a specific item from the shopping cart by productId.
    /// Returns the updated cart state after removal.
    /// Use this tool when the user explicitly wants to remove an item (alternative to update_cart_item with quantity 0).
    /// </summary>
    [OpenAiToolMetadataAttribute(
        ToolName = "remove_from_cart",
        Visibility = "public",
        InvokingMessage = "Eliminando item del carrito...",
        InvokedMessage = "Item eliminado.")]
    [Description("Removes a specific item from the cart by productId. Returns updated cart state after removal.")]
    public async Task<CallToolResult> RemoveFromCart(
        McpServer server,
        [Description("Cart identifier (string, required). Use the cartId from a previous cart operation to maintain the same cart session.")] string cartId,
        [Description("Product ID (string, required) of the item to remove. Must match a productId from an existing cart item.")] string productId,
        CancellationToken cancellationToken = default)
    {
        var command = new RemoveFromCartCommand(cartId, productId);
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
            ["openai/toolInvocation/invoking"] = "Eliminando item del carrito...",
            ["openai/toolInvocation/invoked"] = "Item eliminado.",
            ["openai/widgetSessionId"] = response.CartId
        };

        var message = $"Item eliminado del carrito {response.CartId}. Total: {response.TotalItems} artículo(s).";

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
