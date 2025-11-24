using System.ComponentModel.DataAnnotations;

namespace courses_buynsell_api.DTOs.User
{
    public class AddAdminRequest
    {
        [Required(ErrorMessage = "Full name is required.")]
        [MaxLength(100, ErrorMessage = "Full name must be less than 100 characters.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [MaxLength(100, ErrorMessage = "Email must be less than 100 characters.")]
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[a-zA-Z]{2,}$",
    ErrorMessage = "Invalid email format (e.g. name@example.com).")]
        public string Email { get; set; } = string.Empty;

        [MaxLength(11, ErrorMessage = "Phone number must be less than 11 characters.")]
        [MinLength(10, ErrorMessage = "Phone number must be at least 10 characters.")]
        [Phone(ErrorMessage = "Invalid phone number format.")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
        [MaxLength(50, ErrorMessage = "Password must be less than 50 characters.")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d@$!%*?&]+$",
            ErrorMessage = "Password must contain at least one letter and one number.")]
        public string Password { get; set; } = string.Empty;
    }
}