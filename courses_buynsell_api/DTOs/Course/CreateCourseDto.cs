using System.ComponentModel.DataAnnotations;

namespace courses_buynsell_api.DTOs.Course;

public class CreateCourseDto
{
    [Required] public string Title { get; set; } = string.Empty;
    [Required] public string TeacherName { get; set; } = string.Empty;
    public string? Description { get; set; }
    [Range(0, double.MaxValue)] public decimal Price { get; set; }
    public string Level { get; set; } = string.Empty;
    public IFormFile? Image { get; set; }
    public int DurationHours { get; set; }
    public int CategoryId { get; set; }
    public int SellerId { get; set; }
    public List<CourseContentDto>? CourseContents { get; set; }
    public List<SkillTargetDto>? CourseSkills { get; set; }
    public List<SkillTargetDto>? TargetLearners { get; set; }
}