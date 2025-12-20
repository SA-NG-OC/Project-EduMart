namespace courses_buynsell_api.DTOs.Course;

public class CourseListItemDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? Level { get; set; }
    public string? ImageUrl { get; set; }
    public decimal AverageRating { get; set; }
    public int TotalPurchased { get; set; }
    public string TeacherName { get; set; } = string.Empty;
    public int SellerId { get; set; }
    public int DurationHours { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public bool? IsApproved { get; set; }
    public bool? IsRestricted { get; set; }

    public int CommentCount { get; set; }

    public List<CourseContentDto>? CourseContents { get; set; }
    public List<SkillTargetDto>? CourseSkills { get; set; }
    public List<SkillTargetDto>? TargetLearners { get; set; }
}