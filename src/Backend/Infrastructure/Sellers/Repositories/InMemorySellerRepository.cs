using Domain.Sellers;

namespace Infrastructure.Sellers.Repositories;

public class InMemorySellerRepository : ISellerRepository
{
    private readonly List<Seller> _sellers = new()
    {
        new Seller
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
            Name = "Coolbox",
            Provider = SellerProvider.Vtex,
            ShopKey = "coolbox"
        },
        new Seller
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000003"),
            Name = "Shopstar",
            Provider = SellerProvider.Vtex,
            ShopKey = "mercury"
        },
        new Seller
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000004"),
            Name = "Promart",
            Provider = SellerProvider.Vtex,
            ShopKey = "promart"
        },
        new Seller
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000005"),
            Name = "Plaza Vea",
            Provider = SellerProvider.Vtex,
            ShopKey = "plazavea"
        },
        new Seller
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000006"),
            Name = "OE",
            Provider = SellerProvider.Vtex,
            ShopKey = "oechsle"
        },
        new Seller
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000007"),
            Name = "Wong",
            Provider = SellerProvider.Vtex,
            ShopKey = "wong"
        },
        new Seller
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000008"),
            Name = "Metro",
            Provider = SellerProvider.Vtex,
            ShopKey = "metrope"
        }
    };

    public Task<IEnumerable<Seller>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IEnumerable<Seller>>(_sellers);
    }

    public Task<Seller?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var seller = _sellers.FirstOrDefault(s => s.Id == id);
        return Task.FromResult(seller);
    }

    public Task<Seller?> GetByShopKeyAsync(string shopKey, CancellationToken cancellationToken = default)
    {
        var seller = _sellers.FirstOrDefault(s => s.ShopKey.Equals(shopKey, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(seller);
    }
}
