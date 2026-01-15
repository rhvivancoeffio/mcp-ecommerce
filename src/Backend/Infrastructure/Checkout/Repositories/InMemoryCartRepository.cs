using Domain.Checkout;
using System.Collections.Concurrent;

namespace Infrastructure.Checkout.Repositories;

public class InMemoryCartRepository : ICartRepository
{
    private readonly ConcurrentDictionary<string, Cart> _carts = new();

    public Task<Cart?> GetCartAsync(string cartId, CancellationToken cancellationToken = default)
    {
        _carts.TryGetValue(cartId, out var cart);
        return Task.FromResult(cart);
    }

    public Task<Cart> CreateCartAsync(string cartId, CancellationToken cancellationToken = default)
    {
        var cart = new Cart
        {
            CartId = cartId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Items = new List<CartItem>()
        };
        
        _carts.TryAdd(cartId, cart);
        return Task.FromResult(cart);
    }

    public Task<Cart> UpdateCartAsync(Cart cart, CancellationToken cancellationToken = default)
    {
        cart.UpdatedAt = DateTime.UtcNow;
        _carts.AddOrUpdate(cart.CartId, cart, (key, oldValue) => cart);
        return Task.FromResult(cart);
    }

    public Task<bool> DeleteCartAsync(string cartId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_carts.TryRemove(cartId, out _));
    }

    public Task<bool> CartExistsAsync(string cartId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_carts.ContainsKey(cartId));
    }
}
