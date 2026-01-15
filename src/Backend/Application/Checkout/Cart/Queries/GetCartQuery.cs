using MediatR;
using Domain.Checkout;
using CartEntity = Domain.Checkout.Cart;

namespace Application.Checkout.Cart.Queries;

public record GetCartQuery(
    string CartId
) : IRequest<GetCartResponse>;

public record GetCartResponse(
    string CartId,
    CartEntity Cart,
    int TotalItems
);
