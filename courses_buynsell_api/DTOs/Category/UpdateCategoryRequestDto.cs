using System.ComponentModel.DataAnnotations;
namespace courses_buynsell_api.DTOs.Category;

public class UpdateCategoryRequestDto
{
    [Required]
    public int Id { get; set; }
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
}