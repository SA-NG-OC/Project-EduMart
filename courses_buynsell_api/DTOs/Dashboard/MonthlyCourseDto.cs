namespace courses_buynsell_api.DTOs.Dashboard;

public class MonthlyCourseDto
{
    public int Month { get; set; }  // 1 -> 12
    public int Year { get; set; }
    public decimal TotalRevenue { get; set; }
}