namespace Domain.Checkout;

public class PaymentInfo
{
    public PaymentMethod Method { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public PaymentStatus Status { get; set; }
    public DateTime? ProcessedAt { get; set; }
    
    // Card information (masked)
    public string? CardLastFour { get; set; }
    public string? CardBrand { get; set; }
}

public enum PaymentMethod
{
    CreditCard = 0,
    DebitCard = 1,
    PayPal = 2,
    BankTransfer = 3,
    CashOnDelivery = 4
}

public enum PaymentStatus
{
    Pending = 0,
    Processing = 1,
    Completed = 2,
    Failed = 3,
    Refunded = 4
}
