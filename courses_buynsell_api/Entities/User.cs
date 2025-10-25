using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace courses_buynsell_api.Entities;

public class User
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = String.Empty;
    [Required]
    [MaxLength(100)]
    public string Email { get; set; } = String.Empty;
    [Required] public string PasswordHash { get; set; } = String.Empty;
    [Required]
    [MaxLength(20)]
    public string Role { get; set; } = String.Empty;
    [Required] 
    public DateTime CreatedAt { get; set; }
    public Cart? Cart { get; set; }
    public ICollection<Course> Courses { get; set; } =  new List<Course>();
    public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}