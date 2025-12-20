using System.ComponentModel.DataAnnotations;

namespace courses_buynsell_api.DTOs.Chat;

public class SendMessageDto
{
    [Required]
    public int ConversationId { get; set; }

    [Required]
    [MaxLength(5000)]
    public string Content { get; set; } = string.Empty;
}