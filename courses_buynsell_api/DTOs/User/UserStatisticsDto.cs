namespace courses_buynsell_api.DTOs.User;

public class UserStatisticsDto
{
    public long TotalUsers { get; set; }
    public Dictionary<string, long> RoleCounts { get; set; } = new();
}
