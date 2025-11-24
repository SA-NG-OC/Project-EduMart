using System.ComponentModel.DataAnnotations;

namespace courses_buynsell_api.DTOs.Momo;

public class CreateMomoPaymentRequestDto
{
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
    public decimal Amount { get; set; }
    [Required]
    [MinLength(1, ErrorMessage = "At least one course ID must be provided.")]
    public List<int> CourseIds { get; set; } = new();
}
