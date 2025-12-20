using System.ComponentModel.DataAnnotations;

namespace courses_buynsell_api.DTOs.Chat;

public class GetMessagesDto
{
    public int ConversationId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}