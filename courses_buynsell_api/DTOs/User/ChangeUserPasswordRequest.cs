using System.ComponentModel.DataAnnotations;

namespace courses_buynsell_api.DTOs.User;

public class ChangeUserPasswordRequest
{

    [Required(ErrorMessage = "Current password is required.")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
    [MaxLength(50, ErrorMessage = "Password must be less than 50 characters.")]
    public string NewPassword { get; set; } = string.Empty;
}