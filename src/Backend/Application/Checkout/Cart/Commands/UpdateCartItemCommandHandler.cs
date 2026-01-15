using MediatR;
using Domain.Checkout;
using Application.Checkout.Cart.Commands;
using CartEntity = Domain.Checkout.Cart;

namespace Application.Checkout.Cart.Commands;

public class UpdateCartItemCommandHandler : IRequestHandler<UpdateCartItemCommand, UpdateCartItemResponse>
{
    private readonly ICartRepository _cartRepository;

    public UpdateCartItemCommandHandler(ICartRepository cartRepository)
    {
        _cartRepository = cartRepository;
    }

    public async Task<UpdateCartItemResponse> Handle(UpdateCartItemCommand request, CancellationToken cancellationToken)
    {
        var cart = await _cartRepository.GetCartAsync(request.CartId, cancellationToken)
            ?? throw new InvalidOperationException($"Cart {request.CartId} not found");

        var item = cart.Items.FirstOrDefault(i => i.ProductId == request.ProductId);
        
        if (item == null)
        {
            throw new InvalidOperationException($"Product {request.ProductId} not found in cart");
        }

        if (request.Quantity <= 0)
        {
            // Remove item if quantity is 0 or less
            cart.Items.Remove(item);
        }
        else
        {
            item.Quantity = request.Quantity;
        }

        cart.UpdatedAt = DateTime.UtcNow;
        cart = await _cartRepository.UpdateCartAsync(cart, cancellationToken);

        return new UpdateCartItemResponse(
            CartId: cart.CartId,
            Cart: cart,
            TotalItems: cart.TotalItems
        );
    }
}
