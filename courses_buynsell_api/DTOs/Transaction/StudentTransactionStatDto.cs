namespace courses_buynsell_api.DTOs.Transaction;

public class StudentTransactionStatDto
{
    public int StudentId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public int PurchaseCount { get; set; }
    public decimal TotalRevenue { get; set; }
    public DateTime? LastTransactionDate { get; set; }
}