namespace courses_buynsell_api.DTOs.Review;

public record ReviewResponseDto
{
    public int Id { get; set; }
    public int UserId { get; set; }    // NEW
    public string UserName { get; set; } = string.Empty;
    public string? Image { get; set; } // NEW (Avatar)
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
