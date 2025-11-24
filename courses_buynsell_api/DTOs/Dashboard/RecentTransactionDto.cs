namespace courses_buynsell_api.DTOs.Dashboard
{
    public class RecentTransactionDto
    {
        public int Id { get; set; }
        public decimal TotalAmount { get; set; }
        public string BuyerName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
