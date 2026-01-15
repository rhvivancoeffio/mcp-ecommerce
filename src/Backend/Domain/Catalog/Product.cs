namespace Domain.Catalog;

public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string SKU { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty; // Deprecated: Use ImageUrls instead
    public List<string> ImageUrls { get; set; } = new();
    public int Stock { get; set; }
    public Dictionary<string, string> Attributes { get; set; } = new();
    public List<string> Features { get; set; } = new();
}
