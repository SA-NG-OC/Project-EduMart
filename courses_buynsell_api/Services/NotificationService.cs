using courses_buynsell_api.DTOs.Notification;
using courses_buynsell_api.Entities;
using courses_buynsell_api.Hubs;
using courses_buynsell_api.Interfaces;
using courses_buynsell_api.Data;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace courses_buynsell_api.Services;

public class NotificationService : INotificationService
{
    private readonly AppDbContext _context;
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationService(AppDbContext context, IHubContext<NotificationHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    public async Task<IEnumerable<NotificationDto>> GetNotificationsBySellerIdAsync(int sellerId)
    {
        var notifications = await _context.Notifications
            .Where(n => n.SellerId == sellerId)
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new NotificationDto
            {
                Id = n.Id,
                Message = n.Message,
                CreatedAt = n.CreatedAt,
                IsRead = n.IsRead,
                SellerId = n.SellerId,
            })
            .ToListAsync();

        return notifications;
    }

    public async Task<int> GetUnreadCountAsync(int sellerId)
    {
        return await _context.Notifications
            .CountAsync(n => n.SellerId == sellerId && !n.IsRead);
    }

    public async Task<NotificationDto> CreateNotificationAsync(CreateNotificationDto dto)
    {
        var notification = new Notification
        {
            Message = dto.Message,
            SellerId = dto.SellerId,
            CreatedAt = DateTime.UtcNow,
            IsRead = false
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        var notificationDto = new NotificationDto
        {
            Id = notification.Id,
            Message = notification.Message,
            CreatedAt = notification.CreatedAt,
            IsRead = notification.IsRead,
            SellerId = notification.SellerId
        };

        // Gửi thông báo real-time đến group của seller
        await _hubContext.Clients
            .Group($"seller_{dto.SellerId}")
            .SendAsync("ReceiveNotification", notificationDto);

        return notificationDto;
    }

    public async Task<bool> MarkAsReadAsync(int notificationId, int sellerId)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.SellerId == sellerId);

        if (notification == null)
            return false;

        notification.IsRead = true;
        await _context.SaveChangesAsync();

        // Gửi cập nhật số lượng chưa đọc
        var unreadCount = await GetUnreadCountAsync(sellerId);
        await _hubContext.Clients
            .Group($"seller_{sellerId}")
            .SendAsync("UpdateUnreadCount", unreadCount);

        return true;
    }

    public async Task<bool> MarkAllAsReadAsync(int sellerId)
    {
        var notifications = await _context.Notifications
            .Where(n => n.SellerId == sellerId && !n.IsRead)
            .ToListAsync();

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
        }

        await _context.SaveChangesAsync();

        // Gửi cập nhật số lượng chưa đọc (sẽ là 0)
        await _hubContext.Clients
            .Group($"seller_{sellerId}")
            .SendAsync("UpdateUnreadCount", 0);

        return true;
    }

    public async Task<bool> DeleteNotificationAsync(int notificationId, int sellerId)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.SellerId == sellerId);

        if (notification == null)
            return false;

        _context.Notifications.Remove(notification);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAllNotificationsAsync(int sellerId)
    {
        var notifications = await _context.Notifications
            .Where(n => n.SellerId == sellerId)
            .ToListAsync();

        _context.Notifications.RemoveRange(notifications);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task SendCoursePurchaseNotificationAsync(int sellerId, string courseName, string buyerName)
    {
        var message = $"🎉 {buyerName} vừa mua khóa học '{courseName}' của bạn!";

        await CreateNotificationAsync(new CreateNotificationDto
        {
            Message = message,
            SellerId = sellerId
        });
    }

    public async Task SendPaymentSuccessNotificationAsync(int sellerId, decimal amount, string courseName)
    {
        var message = $"💰 Bạn đã nhận được {amount:N0} VNĐ từ khóa học '{courseName}'";

        await CreateNotificationAsync(new CreateNotificationDto
        {
            Message = message,
            SellerId = sellerId
        });
    }
}