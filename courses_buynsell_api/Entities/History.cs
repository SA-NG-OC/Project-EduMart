using System.ComponentModel.DataAnnotations;

namespace courses_buynsell_api.Entities;

public class History
{
    [Required]
    public int UserId { get; set; }
    public User? User { get; set; }

    [Required]
    public int CourseId { get; set; }
    public Course? Course { get; set; }

    public DateTime CreatedAt { get; set; }
}