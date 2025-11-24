namespace courses_buynsell_api.DTOs.Course;

public class CourseContentDto
{
    public int Id { get; set; } // 0 => new
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; }
}