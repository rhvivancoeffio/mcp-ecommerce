using MediatR;
using Domain.Checkout;
using Application.Checkout.Cart.Queries;
using CartEntity = Domain.Checkout.Cart;

namespace Application.Checkout.Cart.Queries;

public class GetCartQueryHandler : IRequestHandler<GetCartQuery, GetCartResponse>
{
    private readonly ICartRepository _cartRepository;

    public GetCartQueryHandler(ICartRepository cartRepository)
    {
        _cartRepository = cartRepository;
    }

    public async Task<GetCartResponse> Handle(GetCartQuery request, CancellationToken cancellationToken)
    {
        var cart = await _cartRepository.GetCartAsync(request.CartId, cancellationToken);
        
        if (cart == null)
        {
            // Create empty cart if it doesn't exist
            cart = await _cartRepository.CreateCartAsync(request.CartId, cancellationToken);
        }

        return new GetCartResponse(
            CartId: cart.CartId,
            Cart: cart,
            TotalItems: cart.TotalItems
        );
    }
}
