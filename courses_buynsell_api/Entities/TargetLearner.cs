using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace courses_buynsell_api.Entities;

public class TargetLearner
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [Required]
    [MaxLength(200)]
    public string Description { get; set; } = String.Empty;
    public int CourseId { get; set; }
    public Course? Course { get; set; }
}