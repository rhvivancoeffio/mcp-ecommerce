namespace Domain.Sellers;

public class Seller
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public SellerProvider Provider { get; set; }
    public string ShopKey { get; set; } = string.Empty;
}

public enum SellerProvider
{
    Vtex = 0,
    Shopify = 1
}
