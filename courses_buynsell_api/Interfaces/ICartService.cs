using courses_buynsell_api.DTOs.Cart;

namespace courses_buynsell_api.Interfaces
{
    public interface ICartService
    {
        // GET: Trả về danh sách CartItemDto 
        Task<List<CartItemDto>> GetMyCartDetailsAsync(int userId);

        // POST: Trả về CartDto 
        Task<CartDto> AddToCartAsync(int userId, int courseId);

        Task RemoveFromCartAsync(int userId, int courseId);
        Task ClearCartAsync(int userId);
    }
}