using System.ComponentModel;
using MediatR;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using Application.Checkout.Cart.Commands;
using System.Text.Json.Nodes;
using Server.Common.Mcp.Attributes;

namespace Server.Checkout.McpTools;

public sealed class UpdateCartItemTool
{
    private readonly IMediator _mediator;

    public UpdateCartItemTool(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Updates the quantity of a specific item in the shopping cart.
    /// If quantity is set to 0, the item is removed from the cart.
    /// Returns the updated cart state with all items and totals.
    /// Use this tool to change item quantities or remove items by setting quantity to 0.
    /// </summary>
    [OpenAiToolMetadataAttribute(
        ToolName = "update_cart_item",
        Visibility = "public",
        InvokingMessage = "Actualizando item del carrito...",
        InvokedMessage = "Item actualizado.")]
    [Description("Updates the quantity of a specific item in the cart. Set quantity to 0 to remove the item. Returns updated cart state.")]
    public async Task<CallToolResult> UpdateCartItem(
        McpServer server,
        [Description("Cart identifier (string, required). Use the cartId from a previous cart operation (add_to_cart, get_cart, etc.) to maintain the same cart session.")] string cartId,
        [Description("Product ID (string, required) of the item to update. Must match a productId from an existing cart item.")] string productId,
        [Description("New quantity (int, required). Must be >= 0. Set to 0 to remove the item from the cart. Set to a positive number to update the quantity.")] int quantity,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateCartItemCommand(cartId, productId, quantity);
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
            ["openai/toolInvocation/invoking"] = "Actualizando item del carrito...",
            ["openai/toolInvocation/invoked"] = "Item actualizado.",
            ["openai/widgetSessionId"] = response.CartId
        };

        var message = quantity == 0
            ? $"Item eliminado del carrito {response.CartId}."
            : $"Cantidad actualizada en carrito {response.CartId}. Total: {response.TotalItems} artículo(s).";

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
