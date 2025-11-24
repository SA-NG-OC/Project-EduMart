namespace courses_buynsell_api.DTOs.Notification;

public class NotificationDto
{
    public int Id { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsRead { get; set; }
    public int SellerId { get; set; }
}