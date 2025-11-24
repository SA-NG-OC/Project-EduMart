namespace courses_buynsell_api.DTOs.Dashboard;

public class MonthlyBuyerStatsDto
{
    public string Month { get; set; } = string.Empty; // yyyy-MM format
    public int BuyerCount { get; set; }
}