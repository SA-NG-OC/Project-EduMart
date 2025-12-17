using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace courses_buynsell_api.Entities;

public class Conversation
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public int CourseId { get; set; }
    public Course? Course { get; set; }

    [Required]
    public int BuyerId { get; set; }
    public User? Buyer { get; set; }

    [Required]
    public int SellerId { get; set; }
    public User? Seller { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }

    public DateTime LastMessageAt { get; set; }

    [Required]
    public bool IsVisible { get; set; } = true;

    public ICollection<Message>? Messages { get; set; }
}
