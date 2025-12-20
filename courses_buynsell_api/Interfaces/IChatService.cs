using courses_buynsell_api.DTOs.Chat;
using courses_buynsell_api.DTOs;

namespace courses_buynsell_api.Interfaces;

public interface IChatService
{
    // Tạo hoặc lấy conversation giữa buyer và seller về một course
    Task<ConversationDto> GetOrCreateConversationAsync(int buyerId, CreateConversationDto dto);

    // Lấy danh sách conversations của user (buyer hoặc seller) - phân trang
    Task<PagedResult<ConversationDto>> GetUserConversationsAsync(int userId, int page, int pageSize);

    // Lấy danh sách conversations về một course cụ thể (cho seller) - phân trang
    Task<PagedResult<ConversationDto>> GetCourseConversationsAsync(int sellerId, int courseId, int page, int pageSize);

    // Gửi message
    Task<MessageDto> SendMessageAsync(int senderId, SendMessageDto dto);

    // Lấy messages của một conversation (có phân trang)
    Task<PagedResult<MessageDto>> GetConversationMessagesAsync(int userId, GetMessagesDto dto);

    // Đánh dấu messages đã đọc
    Task MarkMessagesAsReadAsync(int userId, int conversationId);

    // Kiểm tra user có quyền truy cập conversation không
    Task<bool> HasAccessToConversationAsync(int userId, int conversationId);

    // Lấy conversation detail
    Task<ConversationDto?> GetConversationDetailAsync(int userId, int conversationId);

    // Đếm số tin nhắn chưa đọc
    Task<int> GetUnreadCountAsync(int userId);
    Task<int> CountUnreadConversationsAsync(int userId);
    Task<List<UnreadConversationCountDto>> GetUnreadConversationsByCourseAsync(int sellerId);
    // Thêm method này vào interface IChatService hiện tại
    Task<SendMessageWithConversationResponseDto> SendMessageWithNewConversationAsync(
        int buyerId,
        SendMessageWithNewConversationDto dto
    );
    Task<List<ChatUserSearchResultDto>> SearchUsersAsync(int currentUserId, string query);
    Task<bool> HideConversationAsync(int userId, int conversationId);
}