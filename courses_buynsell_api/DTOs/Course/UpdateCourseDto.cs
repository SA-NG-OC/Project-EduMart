using System.ComponentModel.DataAnnotations;

namespace courses_buynsell_api.DTOs.Course;

public class UpdateCourseDto
{
    [Required] public string? Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    [Range(0, double.MaxValue)] public decimal? Price { get; set; }
    public string? Level { get; set; }
    public string? TeacherName { get; set; }
    public IFormFile? Image { get; set; }
    public int? DurationHours { get; set; }
    public int? CategoryId { get; set; }
    
    public List<CourseContentDto>? CourseContents { get; set; }
    public List<SkillTargetDto>? CourseSkills { get; set; }
    public List<TargetLearnerDto>? TargetLearners { get; set; }
}