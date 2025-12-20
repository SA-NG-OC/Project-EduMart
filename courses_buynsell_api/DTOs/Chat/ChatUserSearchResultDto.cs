namespace courses_buynsell_api.DTOs.Chat;

public class ChatUserSearchResultDto
{
    // ID của cuộc hội thoại (Quan trọng nhất để FE mở chat)
    public int ConversationId { get; set; }

    // Thông tin người mua để hiển thị tên và avatar
    public int BuyerId { get; set; }
    public string BuyerName { get; set; } = string.Empty;
    public string BuyerAvatar { get; set; } = string.Empty;

    // Thông tin khóa học (Để phân biệt nếu khách mua nhiều khóa)
    public string CourseTitle { get; set; } = string.Empty;

    // (Tùy chọn) Thời gian tin nhắn cuối để hiển thị cho xịn
    public DateTime LastMessageAt { get; set; }
}