using courses_buynsell_api.Data;
using courses_buynsell_api.DTOs;
using courses_buynsell_api.DTOs.History;
using courses_buynsell_api.Entities;
using courses_buynsell_api.Exceptions;
using courses_buynsell_api.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace courses_buynsell_api.Services
{
    public class HistoryService : IHistoryService
    {
        private readonly AppDbContext _context;

        public HistoryService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<HistoryItemDto>> GetHistoryAsync(int userId, int page, int pageSize)
        {
            // 1. Tạo Query cơ bản
            var query = _context.Histories
                .Include(h => h.Course)
                .ThenInclude(c => c.Category)
                .Include(h => h.Course.Enrollments)
                .Where(h => h.UserId == userId);

            // 2. Tính tổng số lượng bản ghi
            int totalCount = await query.CountAsync();

            // 3. Thực hiện phân trang và lấy dữ liệu
            var items = await query
                .OrderByDescending(h => h.CreatedAt) // Mới xem nhất lên đầu
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(h => new HistoryItemDto
                {
                    Id = h.CourseId, // Map CourseId vào Id theo mẫu JSON
                    Title = h.Course.Title,
                    Description = h.Course.Description ?? "",
                    Price = h.Course.Price,
                    Level = h.Course.Level,
                    ImageUrl = h.Course.ImageUrl,
                    AverageRating = h.Course.AverageRating,
                    TotalPurchased = h.Course.TotalPurchased,
                    TeacherName = h.Course.TeacherName,
                    SellerId = h.Course.SellerId,
                    DurationHours = h.Course.DurationHours,
                    CategoryName = h.Course.Category != null ? h.Course.Category.Name : "Uncategorized",
                    IsApproved = h.Course.IsApproved,
                    IsRestricted = false 
                })
                .ToListAsync();

            // 4. Trả về kết quả phân trang
            return new PagedResult<HistoryItemDto>
            {
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                Items = items
            };
        }

        public async Task AddToHistoryAsync(int userId, int courseId)
        {
            // Tìm khóa tổ hợp
            var existingHistory = await _context.Histories.FindAsync(userId, courseId);

            if (existingHistory != null)
            {
                existingHistory.CreatedAt = DateTime.UtcNow;
            }
            else
            {
                var newHistory = new History
                {
                    UserId = userId,
                    CourseId = courseId,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Histories.Add(newHistory);
            }
            await _context.SaveChangesAsync();
        }

        public async Task RemoveFromHistoryAsync(int userId, int courseId)
        {
            var historyItem = await _context.Histories.FindAsync(userId, courseId);
            if (historyItem != null)
            {
                _context.Histories.Remove(historyItem);
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new NotFoundException("Khóa học không tồn tại trong lịch sử xem.");
            }
        }

        public async Task ClearHistoryAsync(int userId)
        {
            var items = await _context.Histories
                .Where(h => h.UserId == userId)
                .ToListAsync();

            if (items.Any())
            {
                _context.Histories.RemoveRange(items);
                await _context.SaveChangesAsync();
            }
        }
    }
}