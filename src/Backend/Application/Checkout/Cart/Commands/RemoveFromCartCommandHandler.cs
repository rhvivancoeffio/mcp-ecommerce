using MediatR;
using Domain.Checkout;
using Application.Checkout.Cart.Commands;
using CartEntity = Domain.Checkout.Cart;

namespace Application.Checkout.Cart.Commands;

public class RemoveFromCartCommandHandler : IRequestHandler<RemoveFromCartCommand, RemoveFromCartResponse>
{
    private readonly ICartRepository _cartRepository;

    public RemoveFromCartCommandHandler(ICartRepository cartRepository)
    {
        _cartRepository = cartRepository;
    }

    public async Task<RemoveFromCartResponse> Handle(RemoveFromCartCommand request, CancellationToken cancellationToken)
    {
        var cart = await _cartRepository.GetCartAsync(request.CartId, cancellationToken)
            ?? throw new InvalidOperationException($"Cart {request.CartId} not found");

        var item = cart.Items.FirstOrDefault(i => i.ProductId == request.ProductId);
        
        if (item == null)
        {
            throw new InvalidOperationException($"Product {request.ProductId} not found in cart");
        }

        cart.Items.Remove(item);
        cart.UpdatedAt = DateTime.UtcNow;
        cart = await _cartRepository.UpdateCartAsync(cart, cancellationToken);

        return new RemoveFromCartResponse(
            CartId: cart.CartId,
            Cart: cart,
            TotalItems: cart.TotalItems
        );
    }
}
