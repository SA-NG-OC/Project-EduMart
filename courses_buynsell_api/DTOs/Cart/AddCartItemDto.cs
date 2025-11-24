namespace courses_buynsell_api.DTOs.Cart;

public class AddCartItemDto
{
    public int? UserId { get; set; }
    public int CourseId { get; set; }
}