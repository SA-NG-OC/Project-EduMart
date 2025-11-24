namespace courses_buynsell_api.DTOs.Transaction;

public class CourseTransactionStatDto
{
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public int PurchaseCount { get; set; }
    public decimal TotalRevenue { get; set; }
    public DateTime? LastTransactionDate { get; set; }
}