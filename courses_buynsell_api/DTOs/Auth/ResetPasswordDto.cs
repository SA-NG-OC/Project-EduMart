using System.ComponentModel.DataAnnotations;

namespace courses_buynsell_api.DTOs.Auth;

public class ResetPasswordDto
{
    [Required]
    public string OTP { get; set; } = string.Empty;

    [Required, MinLength(6)]
    public string NewPassword { get; set; } = string.Empty;
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    [MaxLength(100, ErrorMessage = "Email must be less than 100 characters.")]
    [RegularExpression(@"^[^@\s]+@[^@\s]+\.[a-zA-Z]{2,}$",
    ErrorMessage = "Invalid email format (e.g. name@example.com).")]
    public string Email { get; set; } = string.Empty;
}