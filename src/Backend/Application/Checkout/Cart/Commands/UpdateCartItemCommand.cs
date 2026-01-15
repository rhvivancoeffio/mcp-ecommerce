using MediatR;
using Domain.Checkout;
using CartEntity = Domain.Checkout.Cart;

namespace Application.Checkout.Cart.Commands;

public record UpdateCartItemCommand(
    string CartId,
    string ProductId,
    int Quantity
) : IRequest<UpdateCartItemResponse>;

public record UpdateCartItemResponse(
    string CartId,
    CartEntity Cart,
    int TotalItems
);
