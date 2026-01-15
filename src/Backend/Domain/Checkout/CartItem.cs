namespace Domain.Checkout;

public class CartItem
{
    // Support both string and Guid ProductId for compatibility
    public string ProductId { get; set; } = string.Empty;
    
    // For backward compatibility with CheckoutSession
    public Guid ProductIdGuid => Guid.TryParse(ProductId, out var guid) ? guid : Guid.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string ProductSku { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice => UnitPrice * Quantity;
    public string? ImageUrl { get; set; }
    
    // Additional product information
    public string? Category { get; set; }
    public string? Brand { get; set; }
    
    // Additional metadata
    public Dictionary<string, string> Metadata { get; set; } = new();
}
