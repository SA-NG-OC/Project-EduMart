using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace courses_buynsell_api.Entities;

public class Notification
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Message { get; set; } = string.Empty;
    [Required]
    public DateTime CreatedAt { get; set; }
    public bool IsRead { get; set; }
    public int SellerId { get; set; }
    public User? Seller { get; set; }
}