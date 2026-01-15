using System.Linq;

namespace Domain.Checkout;

public class Cart
{
    public string CartId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<CartItem> Items { get; set; } = new();
    
    public decimal SubTotal => Items.Sum(item => item.TotalPrice);
    public decimal Tax => SubTotal * 0.1m; // 10% tax
    public decimal Shipping => SubTotal > 50 ? 0 : 5.99m; // Free shipping over $50
    public decimal Total => SubTotal + Tax + Shipping;
    
    public int TotalItems => Items.Sum(item => item.Quantity);
    
    // Additional metadata
    public Dictionary<string, string> Metadata { get; set; } = new();
}
