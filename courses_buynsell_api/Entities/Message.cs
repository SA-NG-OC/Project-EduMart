using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace courses_buynsell_api.Entities;

public class Message
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public int ConversationId { get; set; }
    public Conversation? Conversation { get; set; }

    [Required]
    public int SenderId { get; set; }
    public User? Sender { get; set; }

    [Required]
    public string Content { get; set; } = string.Empty;

    [Required]
    public DateTime CreatedAt { get; set; }

    public bool IsRead { get; set; }
}
