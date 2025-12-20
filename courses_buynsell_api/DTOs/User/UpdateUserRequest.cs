using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace courses_buynsell_api.DTOs.User
{
    [JsonObject(MemberSerialization.OptIn)]
    public class UpdateUserRequest : IValidatableObject
    {
        [JsonProperty("fullName")]
        [MaxLength(100, ErrorMessage = "Full name cannot exceed 100 characters.")]
        public string? FullName { get; set; }

        [JsonProperty("email")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [MaxLength(100, ErrorMessage = "Email must be less than 100 characters.")]
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[a-zA-Z]{2,}$",
            ErrorMessage = "Invalid email format (e.g. name@example.com).")]
        public string? Email { get; set; }

        [JsonProperty("phoneNumber")]
        [Phone(ErrorMessage = "Invalid phone number format.")]
        [MaxLength(15, ErrorMessage = "Phone number must be less than 15 characters.")]
        [MinLength(9, ErrorMessage = "Phone number must be at least 9 characters.")]
        public string? PhoneNumber { get; set; }

        [JsonProperty("avatar")]
        public IFormFile? Avatar { get; set; }
        public bool deleteImage { get; set; } = false;

        // ✅ Custom validation: ít nhất phải có 1 trường hợp được gửi lên để update
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(FullName)
                && string.IsNullOrWhiteSpace(Email)
                && string.IsNullOrWhiteSpace(PhoneNumber))
            {
                yield return new ValidationResult(
                    "At least one field must be provided to update.",
                    new[] { nameof(FullName), nameof(Email), nameof(PhoneNumber) }
                );
            }
        }
    }
}
