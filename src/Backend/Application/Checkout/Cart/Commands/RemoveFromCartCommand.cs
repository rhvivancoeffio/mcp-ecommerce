using MediatR;
using Domain.Checkout;
using CartEntity = Domain.Checkout.Cart;

namespace Application.Checkout.Cart.Commands;

public record RemoveFromCartCommand(
    string CartId,
    string ProductId
) : IRequest<RemoveFromCartResponse>;

public record RemoveFromCartResponse(
    string CartId,
    CartEntity Cart,
    int TotalItems
);
