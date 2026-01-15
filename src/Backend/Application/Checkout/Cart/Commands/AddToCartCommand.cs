using MediatR;
using Domain.Checkout;

namespace Application.Checkout.Cart.Commands;

// Alias to avoid namespace conflict
using CartEntity = Domain.Checkout.Cart;

public record AddToCartCommand(
    string? CartId,
    List<CartItemInput> Items
) : IRequest<AddToCartResponse>;

public record CartItemInput(
    string ProductId,
    string ProductName,
    string ProductSku,
    int Quantity,
    decimal UnitPrice,
    string? ImageUrl = null,
    string? Category = null,
    string? Brand = null,
    Dictionary<string, string>? Metadata = null
);

public record AddToCartResponse(
    string CartId,
    CartEntity Cart,
    int TotalItems
);
