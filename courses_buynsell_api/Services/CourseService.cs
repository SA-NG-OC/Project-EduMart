using courses_buynsell_api.Data;
using courses_buynsell_api.DTOs;
using courses_buynsell_api.DTOs.Course;
using courses_buynsell_api.DTOs.Notification;
using courses_buynsell_api.Entities;
using courses_buynsell_api.Exceptions;
using courses_buynsell_api.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace courses_buynsell_api.Services
{
    public class CourseService : ICourseService
    {
        private readonly AppDbContext _context;
        private readonly IImageService imageService;
        private readonly INotificationService notificationService;
        public CourseService(AppDbContext context, IImageService imageService, INotificationService notificationService)
        {
            _context = context;
            this.imageService = imageService;
            this.notificationService = notificationService;
        }
        public async Task ApproveCourseAsync(int courseId)
        {
            // 1. Tìm khóa học trong database
            var course = await _context.Courses.FindAsync(courseId);

            if (course == null)
            {
                throw new NotFoundException($"Không tìm thấy khóa học với ID: {courseId}");
            }

            if (course.IsApproved)
            {
                return;
            }

            // 4. Cập nhật trạng thái
            course.IsApproved = true;
            course.UpdatedAt = DateTime.UtcNow; // Cập nhật thời gian sửa đổi

            // 5. Lưu thay đổi
            await _context.SaveChangesAsync();
        }

        public async Task<CourseDetailDto> CreateAsync(CreateCourseDto dto, int userId)
        {
            var entity = new Course
            {
                Title = dto.Title,
                Description = dto.Description ?? "",
                Price = dto.Price,
                Level = dto.Level,
                TeacherName = dto.TeacherName,
                DurationHours = dto.DurationHours,
                CategoryId = dto.CategoryId,
                SellerId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsApproved = false,
                IsRestricted = false,
                CourseLecture = dto.CourseLecture

            };

            if (dto.CourseContents != null)
            {
                foreach (var c in dto.CourseContents)
                {
                    entity.CourseContents.Add(new CourseContent { Title = c.Title, Description = c.Description });
                }
            }

            if (dto.CourseSkills != null)
            {
                foreach (var c in dto.CourseSkills)
                {
                    entity.CourseSkills.Add(new CourseSkill { Name = c.Description });
                }
            }

            if (dto.TargetLearners != null)
            {
                foreach (var c in dto.TargetLearners)
                {
                    entity.TargetLearners.Add(new TargetLearner { Description = c.Description });
                }
            }

            if (dto.Image != null)
            {
                entity.ImageUrl = await imageService.UploadImageAsync(dto.Image);
            }

            _context.Courses.Add(entity);

            var notificationMessage = $"Khóa học '{entity.Title}' đã được tạo thành công và đang chờ duyệt.";

            await notificationService.CreateNotificationAsync(new CreateNotificationDto
            {
                SellerId = userId, // Gửi chính người tạo (Seller)
                Message = notificationMessage
            });

            await _context.SaveChangesAsync();

            return await GetByIdAsync(entity.Id, userId) ?? throw new InvalidOperationException("Created but cannot retrieve");
        }

        public async Task DeleteCourseAsync(int courseId, int userId)
        {
            var course = await _context.Courses.FindAsync(courseId);

            if (course == null)
            {
                throw new NotFoundException($"Không tìm thấy khóa học với ID: {courseId}");
            }

            // Kiểm tra quyền sở hữu: Chỉ người tạo ra khóa học mới được xóa
            if (course.SellerId != userId)
            {
                throw new UnauthorizedException("Bạn không có quyền xóa khóa học này.");
            }

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();
        }

        public async Task<PagedResult<CourseListItemDto>> GetCoursesAsync(CourseQueryParameters q)
        {
            var query = _context.Courses.AsQueryable();
            if (!(q.IncludeUnapproved ?? false))
                query = query.Where(c => c.IsApproved);

            if (!(q.IncludeRestricted ?? false))
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

            query = query
                .Include(c => c.Category)
                .Include(c => c.CourseContents)
                .Include(c => c.CourseSkills)
                .Include(c => c.TargetLearners);

            var total = await query.LongCountAsync();
            var items = await query
                .Skip((q.Page - 1) * q.PageSize)
                .Take(q.PageSize)
                .Select(c => new CourseListItemDto
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
                    IsApproved = c.IsApproved,
                    IsRestricted = c.IsRestricted,
                    CommentCount = c.Enrollments.Count,
                    CourseContents = c.CourseContents.Select(c => new CourseContentDto { Id = c.Id, Title = c.Title, Description = c.Description ?? "" }).ToList(),
                    CourseSkills = c.CourseSkills.Select(c => new SkillTargetDto { Id = c.Id, Description = c.Name }).ToList(),
                    TargetLearners = c.TargetLearners.Select(c => new SkillTargetDto { Id = c.Id, Description = c.Description }).ToList()
                })
                .ToListAsync();
            return new PagedResult<CourseListItemDto>
            {
                Page = q.Page,
                PageSize = q.PageSize,
                TotalCount = total,
                Items = items,
            };
        }


        public async Task<CourseDetailDto?> GetByIdAsync(int id, int userId)
        {
            var course = await _context.Courses
                .AsNoTracking()
                .Include(c => c.CourseContents)
                .Include(c => c.CourseSkills)
                .Include(c => c.TargetLearners)
                .Include(c => c.Category)
                .Include(c => c.Seller)
                .Include(c => c.Reviews)
                .Include(c => c.Enrollments)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null) return null;

            var commentCount = course.Reviews.Count;

            var result = new CourseDetailDto
            {
                Id = course.Id,
                Title = course.Title,
                Description = course.Description,
                Price = course.Price,
                Level = course.Level,
                TeacherName = course.TeacherName,
                ImageUrl = course.ImageUrl,
                DurationHours = course.DurationHours,
                AverageRating = course.AverageRating,
                TotalPurchased = course.TotalPurchased,
                SellerId = course.SellerId,
                CategoryName = course.Category!.Name,
                IsApproved = course.IsApproved,
                CreatedAt = course.CreatedAt,
                UpdatedAt = course.UpdatedAt,
                Email = course.Seller!.Email,
                Phone = course.Seller.PhoneNumber!,
                CommentCount = commentCount,
                IsRestricted = course.IsRestricted,
                CourseContents = course.CourseContents.Select(c => new CourseContentDto { Id = c.Id, Title = c.Title, Description = c.Description }).ToList(),
                CourseSkills = course.CourseSkills.Select(c => new SkillTargetDto { Id = c.Id, Description = c.Name }).ToList(),
                TargetLearners = course.TargetLearners.Select(c => new SkillTargetDto { Id = c.Id, Description = c.Description }).ToList()
            };

            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                if (!course.IsApproved || course.IsRestricted)
                    return null;
                return result;
            }

            if (user.Role.Equals("Buyer") && (!course.IsApproved || course.IsRestricted)) return null;

            if (user.Role.Equals("Admin") || course.Enrollments.Any(e => e.BuyerId == userId) || course.SellerId == userId)
                result.CourseLecture = course.CourseLecture ?? "No lecture found";
            Console.WriteLine($"Added course lecture with value {result.CourseLecture}");
            return result;
        }

        public async Task<PagedResult<CourseListItemDto>> GetCoursesBySellerIdAsync(int sellerId, int page, int pageSize)
        {
            var query = _context.Courses
                .Where(c => c.SellerId == sellerId);

            int totalCount = await query.CountAsync();
            var items = await query
                .Select(c => new CourseListItemDto
                {
                    Id = c.Id,
                    Title = c.Title,
                    Price = c.Price,
                    ImageUrl = c.ImageUrl,
                    TeacherName = c.TeacherName,
                    AverageRating = c.AverageRating,
                    CategoryName = c.Category.Name,
                    SellerId = c.SellerId,
                    IsApproved = c.IsApproved

                })
                .OrderBy(c => c.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<CourseListItemDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<CourseDetailDto?> UpdateAsync(int id, UpdateCourseDto dto, int sellerId)
        {
            var entity = await _context.Courses
            .FirstOrDefaultAsync(c => c.Id == id);

            if (entity == null) return null;

            if (entity.SellerId != sellerId)
                throw new UnauthorizedException("Update your course only!");

            entity.Title = dto.Title ?? entity.Title;
            entity.Description = dto.Description ?? entity.Description;
            entity.Price = dto.Price ?? entity.Price;
            entity.Level = dto.Level ?? entity.Level;
            entity.TeacherName = dto.TeacherName ?? entity.TeacherName;
            entity.DurationHours = dto.DurationHours ?? entity.DurationHours;
            entity.CategoryId = dto.CategoryId ?? entity.CategoryId;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.CourseLecture = dto.CourseLecture;

            if (dto.DeleteImage && !string.IsNullOrEmpty(entity.ImageUrl))
            {
                await imageService.DeleteImageAsync(entity.ImageUrl);
                entity.ImageUrl = null;
            }

            if (dto.Image != null)
            {
                if (!string.IsNullOrEmpty(entity.ImageUrl))
                    await imageService.DeleteImageAsync(entity.ImageUrl);

                entity.ImageUrl = await imageService.UploadImageAsync(dto.Image);
            }

            await _context.SaveChangesAsync();
            return await GetByIdAsync(entity.Id, sellerId);
        }
    }

}
