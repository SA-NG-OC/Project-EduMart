namespace courses_buynsell_api.DTOs.Dashboard;

public class CategoryCourseCountDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int CourseCount { get; set; }
}