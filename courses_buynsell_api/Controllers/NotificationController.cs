using courses_buynsell_api.DTOs.Notification;
using courses_buynsell_api.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace courses_buynsell_api.Controllers;


[ApiController]
[Route("[controller]")]
//[Authorize]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    // GET: /notification/
    [HttpGet]
    [Authorize(Roles = "Seller")]
    public async Task<ActionResult<IEnumerable<NotificationDto>>> GetNotifications()
    {
        int sellerId = HttpContext.Items["UserId"] as int? ?? -1;
        var notifications = await _notificationService.GetNotificationsBySellerIdAsync(sellerId);
        return Ok(notifications);
    }

    // GET: /notification/unread-count/
    [HttpGet("unread-count")]
    [Authorize(Roles = "Seller")]
    public async Task<ActionResult<int>> GetUnreadCount()
    {
        int sellerId = HttpContext.Items["UserId"] as int? ?? -1;
        var count = await _notificationService.GetUnreadCountAsync(sellerId);
        return Ok(count);
    }

    // POST: /notification
    [HttpPost]
    public async Task<ActionResult<NotificationDto>> CreateNotification([FromBody] CreateNotificationDto dto)
    {
        var notification = await _notificationService.CreateNotificationAsync(dto);
        return CreatedAtAction(nameof(GetNotifications), new { sellerId = dto.SellerId }, notification);
    }

    // PUT: /notification/mark-as-read/{notificationId}
    [HttpPut("mark-as-read/{notificationId}")]
    [Authorize(Roles = "Seller")]
    public async Task<ActionResult> MarkAsRead(int notificationId)
    {
        int sellerId = HttpContext.Items["UserId"] as int? ?? -1;
        if (sellerId == -1)
        {
            return Unauthorized(new { message = "User not authenticated." });
        }
        var result = await _notificationService.MarkAsReadAsync(notificationId, sellerId);

        if (!result)
            return NotFound(new { message = "Không tìm thấy thông báo" });

        return Ok(new { message = "Đã đánh dấu đã đọc" });
    }

    // PUT: /notification/mark-all-as-read/
    [HttpPut("mark-all-as-read")]
    [Authorize(Roles = "Seller")]
    public async Task<ActionResult> MarkAllAsRead()
    {
        int sellerId = HttpContext.Items["UserId"] as int? ?? -1;
        await _notificationService.MarkAllAsReadAsync(sellerId);
        return Ok(new { message = "Đã đánh dấu tất cả đã đọc" });
    }

    // DELETE: /notification/{notificationId}
    [HttpDelete("{notificationId}")]
    [Authorize(Roles = "Seller")]
    public async Task<ActionResult> DeleteNotification(int notificationId)
    {
        int sellerId = HttpContext.Items["UserId"] as int? ?? -1;
        if (sellerId == -1)
        {
            return Unauthorized(new { message = "User not authenticated." });
        }
        var result = await _notificationService.DeleteNotificationAsync(notificationId, sellerId);

        if (!result)
            return NotFound(new { message = "Không tìm thấy thông báo" });

        return Ok(new { message = "Đã xóa thông báo" });
    }

    // DELETE: /notification/delete-all/{sellerId}
    [HttpDelete("delete-all/")]
    [Authorize(Roles = "Seller")]
    public async Task<ActionResult> DeleteAllNotifications()
    {
        int sellerId = HttpContext.Items["UserId"] as int? ?? -1;
        await _notificationService.DeleteAllNotificationsAsync(sellerId);
        return Ok(new { message = "Đã xóa tất cả thông báo" });
    }
}