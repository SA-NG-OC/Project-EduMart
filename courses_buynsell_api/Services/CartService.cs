using courses_buynsell_api.Data;
using courses_buynsell_api.DTOs.Cart;
using courses_buynsell_api.Entities;
using courses_buynsell_api.Exceptions;
using courses_buynsell_api.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace courses_buynsell_api.Services
{
    public class CartService : ICartService
    {
        private readonly AppDbContext _context;

        public CartService(AppDbContext context)
        {
            _context = context;
        }

        private async Task<Cart> GetOrCreateCartAsync(int userId)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Course)
                .ThenInclude(co => co.Category)
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Course.Enrollments)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart { UserId = userId, CreatedAt = DateTime.UtcNow };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }
            return cart;
        }

        // GET: 
        public async Task<List<CartItemDto>> GetMyCartDetailsAsync(int userId)
        {
            var cart = await GetOrCreateCartAsync(userId);

            return cart.CartItems.Select(ci => new CartItemDto
            {
                Id = ci.Course.Id, 
                CourseId = ci.CourseId,

                Title = ci.Course.Title,
                Description = ci.Course.Description,
                Price = ci.Course.Price,
                Level = ci.Course.Level,
                ImageUrl = ci.Course.ImageUrl,
                TeacherName = ci.Course.TeacherName,
                SellerId = ci.Course.SellerId,
                DurationHours = ci.Course.DurationHours,
                CategoryName = ci.Course.Category?.Name ?? "Uncategorized",
                IsApproved = ci.Course.IsApproved,
                IsRestricted = false,
                TotalPurchased = ci.Course.TotalPurchased
            }).ToList();
        }

        // POST: 
        public async Task<CartDto> AddToCartAsync(int userId, int courseId)
        {
            var course = await _context.Courses.FindAsync(courseId);
            if (course == null) throw new NotFoundException("Khóa học không tồn tại.");

            var cart = await GetOrCreateCartAsync(userId);

            var existingItem = cart.CartItems.FirstOrDefault(ci => ci.CourseId == courseId);
            if (existingItem != null) throw new BadRequestException("Khóa học đã có trong giỏ hàng.");

            var newItem = new CartItem
            {
                CartId = cart.Id,
                CourseId = courseId,
                AddedAt = DateTime.UtcNow
            };

            _context.CartItems.Add(newItem);
            await _context.SaveChangesAsync();

            return new CartDto
            {
                Id = cart.Id,
                UserId = cart.UserId,
                Items = cart.CartItems.Select(ci => new CartItemDto
                {
                    Id = ci.Id,       // ID của dòng trong bảng CartItem
                    CourseId = ci.CourseId
                }).ToList()
            };
        }

        public async Task RemoveFromCartAsync(int userId, int courseId)
        {
            var cart = await _context.Carts.Include(c => c.CartItems).FirstOrDefaultAsync(c => c.UserId == userId);
            if (cart == null) return;

            var item = cart.CartItems.FirstOrDefault(ci => ci.CourseId == courseId);
            if (item != null)
            {
                _context.CartItems.Remove(item);
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new NotFoundException("Khóa học không có trong giỏ hàng.");
            }
        }

        public async Task ClearCartAsync(int userId)
        {
            var cart = await _context.Carts.Include(c => c.CartItems).FirstOrDefaultAsync(c => c.UserId == userId);
            if (cart != null && cart.CartItems.Any())
            {
                _context.CartItems.RemoveRange(cart.CartItems);
                await _context.SaveChangesAsync();
            }
        }
    }
}