using System.ComponentModel.DataAnnotations;

namespace courses_buynsell_api.DTOs.Course;

public class UpdateCourseDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    [Range(0, double.MaxValue)] public decimal? Price { get; set; }
    public string? Level { get; set; }
    public string? TeacherName { get; set; }
    public IFormFile? Image { get; set; }
    public int? DurationHours { get; set; }
    public int? CategoryId { get; set; }
    public bool DeleteImage { get; set; } = false;
    public string? CourseLecture { get; set; }
}