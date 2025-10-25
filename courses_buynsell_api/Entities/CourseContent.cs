using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace courses_buynsell_api.Entities;

public class CourseContent
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = String.Empty;

    public string? Description { get; set; }

    public int CourseId { get; set; }
    public Course? Course { get; set; }
}