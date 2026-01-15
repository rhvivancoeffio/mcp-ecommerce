namespace Domain.Checkout;

public interface ICartRepository
{
    Task<Cart?> GetCartAsync(string cartId, CancellationToken cancellationToken = default);
    Task<Cart> CreateCartAsync(string cartId, CancellationToken cancellationToken = default);
    Task<Cart> UpdateCartAsync(Cart cart, CancellationToken cancellationToken = default);
    Task<bool> DeleteCartAsync(string cartId, CancellationToken cancellationToken = default);
    Task<bool> CartExistsAsync(string cartId, CancellationToken cancellationToken = default);
}
