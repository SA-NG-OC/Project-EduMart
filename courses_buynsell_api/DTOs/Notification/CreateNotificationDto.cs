using System.ComponentModel.DataAnnotations;
namespace courses_buynsell_api.DTOs.Notification;

public class CreateNotificationDto
{
    [Required]
    public string Message { get; set; } = string.Empty;
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "BuyerId must be a positive integer.")]
    public int SellerId { get; set; }
}