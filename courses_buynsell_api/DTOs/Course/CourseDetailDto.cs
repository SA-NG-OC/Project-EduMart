namespace courses_buynsell_api.DTOs.Course;

public class CourseDetailDto : CourseListItemDto
{
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsApproved { get; set; }
    
    public List<CourseContentDto>? CourseContents { get; set; }
    public List<SkillTargetDto>? CourseSkills { get; set; }
    public List<SkillTargetDto>? TargetLearners { get; set; }
}