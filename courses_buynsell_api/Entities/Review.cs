using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace courses_buynsell_api.Entities;

public class Review
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [Required]
    public int Star { get; set; }
    [Required]
    public DateTime CreatedAt { get; set; }
    public string? Comment { get; set; }
    public int BuyerId { get; set; }
    public User? Buyer { get; set; }
    public int CourseId { get; set; }
    public Course? Course { get; set; }
}