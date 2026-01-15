namespace Domain.Sellers;

public interface ISellerRepository
{
    Task<IEnumerable<Seller>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Seller?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Seller?> GetByShopKeyAsync(string shopKey, CancellationToken cancellationToken = default);
}
