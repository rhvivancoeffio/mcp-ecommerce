namespace Domain.Checkout;

public class Order
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public OrderStatus Status { get; set; }
    public decimal SubTotal { get; set; }
    public decimal Tax { get; set; }
    public decimal Shipping { get; set; }
    public decimal Total { get; set; }
    public string Currency { get; set; } = "USD";
    
    // Customer information
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    
    // Shipping address
    public Address ShippingAddress { get; set; } = new();
    
    // Billing address
    public Address BillingAddress { get; set; } = new();
    
    // Payment information
    public PaymentInfo PaymentInfo { get; set; } = new();
    
    // Order items
    public List<OrderItem> Items { get; set; } = new();
    
    // Additional metadata
    public Dictionary<string, string> Metadata { get; set; } = new();
}

public enum OrderStatus
{
    Pending = 0,
    Processing = 1,
    Shipped = 2,
    Delivered = 3,
    Cancelled = 4,
    Refunded = 5
}
