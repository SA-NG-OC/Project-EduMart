using System.ComponentModel.DataAnnotations;

namespace courses_buynsell_api.DTOs.Chat;

public class ConversationDto
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public string CourseImageUrl { get; set; } = string.Empty;
    public int BuyerId { get; set; }
    public string BuyerName { get; set; } = string.Empty;
    public string BuyerAvatar { get; set; } = string.Empty;
    public int SellerId { get; set; }
    public string SellerName { get; set; } = string.Empty;
    public string SellerAvatar { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime LastMessageAt { get; set; }
    public MessageDto? LastMessage { get; set; }
    public int UnreadCount { get; set; }
}