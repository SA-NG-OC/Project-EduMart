using courses_buynsell_api.DTOs.Favorite;

namespace courses_buynsell_api.Interfaces
{
    public interface IFavoriteService
    {
        Task<List<FavoriteCourseResponse>> GetFavoritesByUserIdAsync(int userId);
        Task<bool> AddFavoriteAsync(int userId, int courseId);
        Task<bool> ClearFavoritesAsync(int userId);
        Task<bool> RemoveFavoriteAsync(int userId, int courseId);
    }
}
