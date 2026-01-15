namespace Domain.Catalog;

public interface IProductRepository
{
    Task<IEnumerable<Product>> GetAllAsync(string shopKey, CancellationToken cancellationToken = default);
    Task<Product?> GetByIdAsync(Guid id, string shopKey, CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetByCategoryAsync(string category, string shopKey, CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> SearchAsync(string searchTerm, string shopKey, CancellationToken cancellationToken = default);
}
