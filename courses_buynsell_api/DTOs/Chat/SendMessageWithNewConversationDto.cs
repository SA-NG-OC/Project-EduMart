using System.ComponentModel.DataAnnotations;

namespace courses_buynsell_api.DTOs.Chat;

public class SendMessageWithNewConversationDto
{
    [Required(ErrorMessage = "Nội dung tin nhắn không được để trống")]
    [StringLength(5000, ErrorMessage = "Tin nhắn không được vượt quá 5000 ký tự")]
    public string Content { get; set; } = string.Empty;

    [Required(ErrorMessage = "SellerId là bắt buộc")]
    public int SellerId { get; set; }

    [Required(ErrorMessage = "CourseId là bắt buộc")]
    public int CourseId { get; set; }
}

public class SendMessageWithConversationResponseDto
{
    public int ConversationId { get; set; }
    public MessageDto Message { get; set; } = null!;
}