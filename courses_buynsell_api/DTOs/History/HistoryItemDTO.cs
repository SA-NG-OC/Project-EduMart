namespace courses_buynsell_api.DTOs.History;

public class HistoryItemDto
{
    public int Id { get; set; } // CourseId
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty; 
    public decimal Price { get; set; }
    public string Level { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public decimal AverageRating { get; set; }
    public int TotalPurchased { get; set; } 
    public string? TeacherName { get; set; }
    public int SellerId { get; set; } 
    public int DurationHours { get; set; } 
    public string? CategoryName { get; set; }
    public bool IsApproved { get; set; } 
    public bool IsRestricted { get; set; } 
}