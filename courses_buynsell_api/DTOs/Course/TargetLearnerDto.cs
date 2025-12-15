using System.ComponentModel.DataAnnotations;

namespace courses_buynsell_api.DTOs.Course
{
    public class TargetLearnerDto
    {
        public int Id { get; set; } 
        [Required] public string Description { get; set; } = string.Empty;
    }
}
