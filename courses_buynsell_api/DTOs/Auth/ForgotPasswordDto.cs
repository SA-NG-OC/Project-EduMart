using System.ComponentModel.DataAnnotations;

namespace courses_buynsell_api.DTOs.Auth;

public class ForgotPasswordDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
}