namespace Domain.Checkout;

public class CheckoutSession
{
    public Guid Id { get; set; }
    public string SessionId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsCompleted { get; set; }
    
    // Cart items
    public List<CartItem> CartItems { get; set; } = new();
    
    // Customer information (optional, can be filled during checkout)
    public string? CustomerEmail { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    
    // Shipping address (optional)
    public Address? ShippingAddress { get; set; }
    
    // Billing address (optional)
    public Address? BillingAddress { get; set; }
    
    // Payment method preference
    public PaymentMethod? PreferredPaymentMethod { get; set; }
    
    // Calculated totals
    public decimal SubTotal { get; set; }
    public decimal Tax { get; set; }
    public decimal Shipping { get; set; }
    public decimal Total { get; set; }
    
    // Metadata
    public Dictionary<string, string> Metadata { get; set; } = new();
}
