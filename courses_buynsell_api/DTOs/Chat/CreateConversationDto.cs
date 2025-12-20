using System.ComponentModel.DataAnnotations;

namespace courses_buynsell_api.DTOs.Chat;

// DTO để tạo hoặc lấy conversation
public class CreateConversationDto
{
    [Required]
    public int CourseId { get; set; }

    [Required]
    public int SellerId { get; set; }
}