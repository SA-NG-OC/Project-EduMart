using System.ComponentModel.DataAnnotations;

namespace courses_buynsell_api.DTOs.Course;

public class CourseContentDto
{
    public int Id { get; set; } // 0 => new
    public string Title { get; set; } = string.Empty;
    [Required] public string Description { get; set; }
}