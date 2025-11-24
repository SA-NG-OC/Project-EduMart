namespace courses_buynsell_api.DTOs.Transaction;

public class TransactionDetailDto
{
    public string TransactionCode { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string BuyerName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public List<TransactionDetailCourseDto> Courses { get; set; } = new();
}