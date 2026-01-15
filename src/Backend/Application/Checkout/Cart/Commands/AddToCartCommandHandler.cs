using MediatR;
using Domain.Checkout;
using Application.Checkout.Cart.Commands;
using System;
using CartEntity = Domain.Checkout.Cart;

namespace Application.Checkout.Cart.Commands;

public class AddToCartCommandHandler : IRequestHandler<AddToCartCommand, AddToCartResponse>
{
    private readonly ICartRepository _cartRepository;

    public AddToCartCommandHandler(ICartRepository cartRepository)
    {
        _cartRepository = cartRepository;
    }

    public async Task<AddToCartResponse> Handle(AddToCartCommand request, CancellationToken cancellationToken)
    {
        // Get or create cart
        CartEntity cart;
        string cartId = request.CartId ?? Guid.NewGuid().ToString("N");
        
        if (!string.IsNullOrEmpty(request.CartId) && await _cartRepository.CartExistsAsync(request.CartId, cancellationToken))
        {
            cart = await _cartRepository.GetCartAsync(request.CartId, cancellationToken) 
                ?? throw new InvalidOperationException($"Cart {request.CartId} not found");
        }
        else
        {
            cart = await _cartRepository.CreateCartAsync(cartId, cancellationToken);
        }

        // Add items to cart
        foreach (var itemInput in request.Items)
        {
            // Check if item already exists in cart
            var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == itemInput.ProductId);
            
            if (existingItem != null)
            {
                // Update quantity
                existingItem.Quantity += itemInput.Quantity;
            }
            else
            {
                // Add new item
                var cartItem = new CartItem
                {
                    ProductId = itemInput.ProductId,
                    ProductName = itemInput.ProductName,
                    ProductSku = itemInput.ProductSku,
                    Quantity = itemInput.Quantity,
                    UnitPrice = itemInput.UnitPrice,
                    ImageUrl = itemInput.ImageUrl,
                    Category = itemInput.Category,
                    Brand = itemInput.Brand,
                    Metadata = itemInput.Metadata ?? new Dictionary<string, string>()
                };
                cart.Items.Add(cartItem);
            }
        }

        cart.UpdatedAt = DateTime.UtcNow;
        cart = await _cartRepository.UpdateCartAsync(cart, cancellationToken);

        return new AddToCartResponse(
            CartId: cart.CartId,
            Cart: cart,
            TotalItems: cart.TotalItems
        );
    }
}
