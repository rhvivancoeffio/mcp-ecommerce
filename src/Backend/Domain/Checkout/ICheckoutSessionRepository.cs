namespace Domain.Checkout;

public interface ICheckoutSessionRepository
{
    Task<CheckoutSession?> GetBySessionIdAsync(string sessionId, CancellationToken cancellationToken = default);
    Task<CheckoutSession> CreateAsync(CheckoutSession session, CancellationToken cancellationToken = default);
    Task<CheckoutSession> UpdateAsync(CheckoutSession session, CancellationToken cancellationToken = default);
    Task DeleteAsync(string sessionId, CancellationToken cancellationToken = default);
}
