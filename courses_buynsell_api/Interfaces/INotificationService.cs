using courses_buynsell_api.DTOs.Notification;

namespace courses_buynsell_api.Interfaces;

public interface INotificationService
{
    // Lấy danh sách thông báo của seller
    Task<IEnumerable<NotificationDto>> GetNotificationsBySellerIdAsync(int sellerId);

    // Lấy số lượng thông báo chưa đọc
    Task<int> GetUnreadCountAsync(int sellerId);

    // Tạo thông báo mới và gửi real-time
    Task<NotificationDto> CreateNotificationAsync(CreateNotificationDto dto);

    // Đánh dấu đã đọc
    Task<bool> MarkAsReadAsync(int notificationId, int sellerId);

    // Đánh dấu tất cả đã đọc
    Task<bool> MarkAllAsReadAsync(int sellerId);

    // Xóa thông báo
    Task<bool> DeleteNotificationAsync(int notificationId, int sellerId);

    // Xóa tất cả thông báo
    Task<bool> DeleteAllNotificationsAsync(int sellerId);

    // Gửi thông báo khi có người mua khóa học
    Task SendCoursePurchaseNotificationAsync(int sellerId, string courseName, string buyerName);

    // Gửi thông báo khi thanh toán thành công
    Task SendPaymentSuccessNotificationAsync(int sellerId, decimal amount, string courseName);
}