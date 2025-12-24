using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace courses_buynsell_api.Entities;

public class Enrollment
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [Required] 
    public DateTime EnrollAt { get; set; }
    public int BuyerId { get; set; }
    public User? Buyer { get; set; }
    public int CourseId { get; set; }
    public Course? Course { get; set; }
}