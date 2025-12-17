using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace courses_buynsell_api.Entities;

public class Course
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    [Required]
    [MaxLength(200)]
    public string Description { get; set; } = string.Empty;
    [Required]
    public decimal Price { get; set; }
    [MaxLength(50)]
    public string Level { get; set; } = string.Empty;
    [Required]
    [MaxLength(200)]
    public string TeacherName { get; set; } = string.Empty;
    public string? ImageUrl { get; set; } = string.Empty;
    [Required]
    public int DurationHours { get; set; }
    public int TotalPurchased { get; set; }
    public decimal AverageRating { get; set; }
    public bool IsApproved { get; set; }
    public bool IsRestricted { get; set; }
    public string? CourseLecture { get; set; } = string.Empty;
    [Required]
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int SellerId { get; set; }
    public User? Seller { get; set; }
    public int CategoryId { get; set; }
    public Category? Category { get; set; }
    public ICollection<CourseSkill> CourseSkills { get; set; } = new List<CourseSkill>();
    public ICollection<CourseContent> CourseContents { get; set; } = new List<CourseContent>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<TargetLearner> TargetLearners { get; set; } = new List<TargetLearner>();
    public ICollection<TransactionDetail> TransactionDetails { get; set; } = new List<TransactionDetail>();
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    public ICollection<History> Histories { get; set; } = new List<History>();
}