using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace courses_buynsell_api.Entities;

public class Block
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int SellerId { get; set; }
    public User? Seller { get; set; }

    // PostgreSQL int[]
    [Column(TypeName = "integer[]")]
    public int[] BlockedUserIds { get; set; } = Array.Empty<int>();
}
