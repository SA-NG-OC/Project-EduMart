namespace courses_buynsell_api.DTOs.Transaction;

public class TransactionListDto
{
    public string TransactionCode { get; set; } = string.Empty;
    public string BuyerName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}