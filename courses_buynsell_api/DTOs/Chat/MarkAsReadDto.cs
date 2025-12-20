using System.ComponentModel.DataAnnotations;

namespace courses_buynsell_api.DTOs.Chat;

public class MarkAsReadDto
{
    [Required]
    public int ConversationId { get; set; }
}