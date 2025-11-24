using courses_buynsell_api.DTOs.Favorite;
using courses_buynsell_api.Data;
using courses_buynsell_api.Entities;
using courses_buynsell_api.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace courses_buynsell_api.Services
{
    public class FavoriteService : IFavoriteService
    {
        private readonly AppDbContext _context;

        public FavoriteService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<FavoriteCourseResponse>> GetFavoritesByUserIdAsync(int userId)
        {
            try
            {
                return await _context.Favorites
                    .Where(f => f.UserId == userId)
                    .Include(f => f.Course)
                    .Select(f => new FavoriteCourseResponse
                    {
                        UserId = f.UserId,
                        CourseId = f.CourseId,
                        Title = f.Course!.Title,
                        Description = f.Course.Description,
                        TeacherName = f.Course.TeacherName,
                        AverageRating = f.Course.AverageRating,
                        TotalPurchased = f.Course.TotalPurchased,
                        DurationHours = f.Course.DurationHours,
                        Price = f.Course.Price,
                        Level = f.Course.Level,
                        ImageUrlUrl = f.Course.ImageUrl
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                // Log lỗi (Serilog, NLog, Console, ...)
                Console.WriteLine($"[ERROR] GetFavoritesByUserIdAsync: {ex.Message}");

                // Có thể throw lên controller hoặc return list rỗng
                throw;
                // hoặc return new List<FavoriteCourseResponse>();
            }
        }

        public async Task<bool> AddFavoriteAsync(int userId, int courseId)
        {
            // Kiểm tra đã tồn tại chưa (tránh trùng khóa học yêu thích)
            var existingFavorite = await _context.Favorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.CourseId == courseId);
            if (existingFavorite != null)
            {
                return false; // Đã tồn tại
            }
            var favorite = new Favorite
            {
                UserId = userId,
                CourseId = courseId
            };
            _context.Favorites.Add(favorite);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ClearFavoritesAsync(int userId)
        {
            var favorites = await _context.Favorites
                .Where(f => f.UserId == userId)
                .ToListAsync();

            if (!favorites.Any())
                return false; // không có gì để xóa

            _context.Favorites.RemoveRange(favorites);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> RemoveFavoriteAsync(int userId, int courseId)
        {
            var favorite = await _context.Favorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.CourseId == courseId);

            if (favorite == null)
                return false; // không có để xóa

            _context.Favorites.Remove(favorite);
            await _context.SaveChangesAsync();
            return true;
        }

    }
}
