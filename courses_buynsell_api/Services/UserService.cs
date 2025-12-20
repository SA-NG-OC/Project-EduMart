using courses_buynsell_api.Data;
using courses_buynsell_api.DTOs.User;
using courses_buynsell_api.Exceptions;
using courses_buynsell_api.Interfaces;
using courses_buynsell_api.Helpers;
using courses_buynsell_api.DTOs;
using courses_buynsell_api.DTOs.Course;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Eventing.Reader;

namespace courses_buynsell_api.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _context;
    private readonly IImageService _imageService;
    private readonly IEmailService _emailService;
    private readonly ICourseService _courseService;

    public UserService(AppDbContext context, IImageService imageService, IEmailService emailService, ICourseService courseService)
    {
        _context = context;
        _imageService = imageService;
        _emailService = emailService;
        _courseService = courseService;
    }

    public async Task<PagedResult<UserListDto>> GetAllUsersAsync(int page, int pageSize)
    {
        var query = _context.Users
            .Select(u => new UserListDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber ?? string.Empty,
                Role = u.Role,
                CreatedAt = u.CreatedAt
            });

        var totalCount = await query.LongCountAsync();

        if (totalCount == 0)
            throw new NotFoundException("No users found.");

        var items = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<UserListDto>
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            Items = items
        };
    }

    public async Task<UserDetailDto> GetUserByIdAsync(int id)
    {
        var user = await _context.Users
            .Where(u => u.Id == id)
            .Select(u => new UserDetailDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                Role = u.Role,
                AvatarUrl = u.AvatarUrl // thêm avatar
            })
            .FirstOrDefaultAsync();

        if (user == null)
        {
            throw new NotFoundException($"User with ID {id} not found.");
        }

        return user;
    }


    public async Task DeleteUserAsync(DeleteUserRequest request)
    {
        var user = await _context.Users.FindAsync(request.Id);
        if (user == null)
        {
            throw new NotFoundException($"User with ID {request.Id} not found.");
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
    }

    public async Task<UserDetailDto> UpdateUserAsync(int id, UpdateUserRequest request)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            throw new NotFoundException($"User with ID {id} not found.");
        }

        // Update text fields
        if (!string.IsNullOrEmpty(request.FullName))
            user.FullName = request.FullName;

        if (!string.IsNullOrEmpty(request.Email))
            user.Email = request.Email;

        if (!string.IsNullOrEmpty(request.PhoneNumber))
            user.PhoneNumber = request.PhoneNumber;

        // Update avatar nếu có file mới
        if (request.Avatar != null)
        {
            // Xóa avatar cũ nếu có
            if (!string.IsNullOrWhiteSpace(user.AvatarUrl))
            {
                await _imageService.DeleteImageAsync(user.AvatarUrl);
            }

            // Upload avatar mới
            var avatarUrl = await _imageService.UploadImageAsync(request.Avatar);
            user.AvatarUrl = avatarUrl;
        }
        // Nếu request.deleteImage = true thì xóa ảnh
        else if (request.deleteImage)
        {
            await _imageService.DeleteImageAsync(user.AvatarUrl);
            user.AvatarUrl = null;
        }


        _context.Users.Update(user);
        await _context.SaveChangesAsync();

        return new UserDetailDto
        {
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Role = user.Role,
            AvatarUrl = user.AvatarUrl
        };
    }


    public async Task<UserDetailDto> AddAdminAsync(AddAdminRequest request)
    {
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email);
        if (existingUser != null)
            throw new BadRequestException("A user with this email already exists.");

        // Tạo token xác thực email
        var verificationToken = TokenHelper.GenerateRefreshToken();

        var newAdmin = new Entities.User
        {
            FullName = request.FullName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            PasswordHash = PasswordHasher.HashPassword(request.Password),
            Role = "Admin",
            IsEmailVerified = true,
            EmailVerificationToken = verificationToken,
            EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24),
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(newAdmin);
        await _context.SaveChangesAsync();

        // Gửi email xác nhận
        await _emailService.SendVerificationEmailAsync(newAdmin.Email, verificationToken);

        return new UserDetailDto
        {
            FullName = newAdmin.FullName,
            Email = newAdmin.Email,
            PhoneNumber = newAdmin.PhoneNumber,
            Role = newAdmin.Role
        };
    }


    public async Task ChangeUserPasswordAsync(ChangeUserPasswordRequest request, int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            throw new NotFoundException("User not found.");
        }

        if (!PasswordHasher.VerifyPassword(request.CurrentPassword, user.PasswordHash))
        {
            throw new BadRequestException("Current password is incorrect.");
        }

        user.PasswordHash = PasswordHasher.HashPassword(request.NewPassword);
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }

    public async Task<PagedResult<UserListDto>> GetUsersByRoleAsync(string role, int page, int pageSize)
    {
        role = role.Trim();

        // kiểm tra role có tồn tại trong DB
        bool roleExists = await _context.Users.AnyAsync(u => u.Role == role);
        if (!roleExists)
        {
            throw new NotFoundException($"Role '{role}' does not exist.");
        }

        // query theo role
        var query = _context.Users
            .Where(u => u.Role == role)
            .Select(u => new UserListDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber ?? "",
                Role = u.Role,
                CreatedAt = u.CreatedAt
            });

        var totalCount = await query.LongCountAsync();

        var items = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<UserListDto>
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            Items = items
        };
    }
    public async Task<UserStatisticsDto> GetUserStatisticsAsync()
    {
        var totalUsers = await _context.Users.LongCountAsync();

        var roleCounts = await _context.Users
            .GroupBy(u => u.Role)
            .Select(g => new
            {
                Role = g.Key,
                Count = g.LongCount()
            })
            .ToDictionaryAsync(x => x.Role, x => x.Count);

        return new UserStatisticsDto
        {
            TotalUsers = totalUsers,
            RoleCounts = roleCounts
        };
    }

    public async Task<PagedResult<CourseListItemUserDto>> GetMyCourses(CourseQueryParameters q, int userId)
    {
        var buyer = await _context.Users.Include(u => u.Enrollments).FirstOrDefaultAsync(u => u.Id == userId);
        if (buyer == null)
            throw new NotFoundException("User not found.");
        var query = _context.Courses.AsQueryable();
        query = query.Where(c => c.Enrollments.Any(e => e.BuyerId == userId));
        query = query.Where(c => c.IsApproved);
        query = query.Where(c => !c.IsRestricted);
        if (q.CategoryId.HasValue)
            query = query.Where(c => c.CategoryId == q.CategoryId);

        if (q.SellerId.HasValue)
            query = query.Where(c => c.SellerId == q.SellerId);

        if (!string.IsNullOrWhiteSpace(q.Level))
            query = query.Where(c => c.Level == q.Level);

        if (q.MinPrice.HasValue)
            query = query.Where(c => c.Price >= q.MinPrice.Value);

        if (q.MaxPrice.HasValue)
            query = query.Where(c => c.Price <= q.MaxPrice.Value);

        if (!string.IsNullOrWhiteSpace(q.Q))
        {
            var text = q.Q.Trim();
            query = query.Where(c =>
                c.Title.Contains(text) ||
                c.Description.Contains(text));
        }

        query = q.SortBy?.ToLower() switch
        {
            "price_asc" => query.OrderBy(c => c.Price),
            "price_desc" => query.OrderByDescending(c => c.Price),
            "rating_desc" => query.OrderByDescending(c => c.AverageRating),
            "popular" => query.OrderByDescending(c => c.TotalPurchased),
            _ => query.OrderByDescending(c => c.CreatedAt)
        };

        query = query.Include(c => c.Category);

        var total = await query.LongCountAsync();
        var items = await query
            .Skip((q.Page - 1) * q.PageSize)
            .Take(q.PageSize)
            .Select(c => new CourseListItemUserDto
            {
                Id = c.Id,
                Title = c.Title,
                Price = c.Price,
                Level = c.Level,
                ImageUrl = c.ImageUrl,
                AverageRating = c.AverageRating,
                TotalPurchased = c.TotalPurchased,
                SellerId = c.SellerId,
                TeacherName = c.TeacherName,
                Description = c.Description,
                DurationHours = c.DurationHours,
                CategoryName = c.Category!.Name,
                EnrollmentDate = _context.Enrollments.Where(e => e.CourseId == c.Id && e.BuyerId == userId).Select(e => e.EnrollAt).FirstOrDefault(),
                IsApproved = c.IsApproved,
                IsRestricted = c.IsRestricted
            })
            .ToListAsync();
        return new PagedResult<CourseListItemUserDto>
        {
            Page = q.Page,
            PageSize = q.PageSize,
            TotalCount = total,
            Items = items,
        };
    }
}