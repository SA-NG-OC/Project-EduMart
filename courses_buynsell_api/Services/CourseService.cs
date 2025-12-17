using courses_buynsell_api.Data;
using courses_buynsell_api.DTOs;
using courses_buynsell_api.DTOs.Course;
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
        public CourseService (AppDbContext context)
        {
            _context = context;
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

        async public Task<CourseDetailDto> CreateCourseAsync(int sellerId, CreateCourseDto request)
        {
            var courseContents = request.CourseContents?.Select(cc => new CourseContent
            {
                Title = cc.Title,
                Description = cc.Description ?? ""
            }).ToList() ?? new List<CourseContent>();
            var courseSkills = request.CourseSkills?.Select(cs => new CourseSkill
            {
                Name = cs.Description
            }).ToList() ?? new List<CourseSkill>();
            var targetLearners = request.TargetLearners?.Select(tl => new TargetLearner
            {
                Description = tl.Description
            }).ToList() ?? new List<TargetLearner>();
            var course = new Course
            {
                Title = request.Title,
                TeacherName = request.TeacherName,
                Description = request.Description ?? "",
                Price = request.Price,
                Level = request.Level,
                DurationHours = request.DurationHours,
                Category = new Category { Name = request.Category },
                SellerId = sellerId,
                CourseContents = courseContents,
                CourseSkills = courseSkills,
                TargetLearners = targetLearners,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsApproved = false
            };
            course.Id = _context.Courses.Add(course).Entity.Id;
            await _context.SaveChangesAsync();
            return await GetCourseByIdAsync(course.Id);
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

        public async Task<PagedResult<CourseListItemDto>> GetAllCoursesAsync(string? keyword, int? categoryId, string? Level, bool? isApproved, decimal? minPrice, decimal? maxPrice, int page, int pageSize)
        {
            var query = _context.Courses.AsQueryable();

            query = query.Where(c => c.IsApproved == true);
            // Filter
            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(c => c.Title.ToLower().Contains(keyword.ToLower()));

            if (categoryId.HasValue)
                query = query.Where(c => c.CategoryId == categoryId);

            if (!string.IsNullOrEmpty(Level) && Level != "All")
            {
                query = query.Where(c => c.Level == Level);
            }

            if (isApproved.HasValue)
            {
                query = query.Where(c => c.IsApproved == isApproved.Value);
            }

            if (minPrice.HasValue)
                query = query.Where(c => c.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(c => c.Price <= maxPrice.Value);

            // Pagination
            int totalCount = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new CourseListItemDto
                {
                    Id = c.Id,
                    Title = c.Title,
                    Price = c.Price,
                    ImageUrl = c.ImageUrl,
                    TeacherName = c.TeacherName,
                    AverageRating = c.AverageRating,
                    CategoryName = c.Category.Name,
                    Level = c.Level,
                    TotalPurchased = c.TotalPurchased,
                    DurationHours = c.DurationHours
                    // Map thêm các trường khác của ListItemDto
                })
                .ToListAsync();

            return new PagedResult<CourseListItemDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<CourseDetailDto> GetCourseByIdAsync(int id)
        {
            var course = await _context.Courses
                .Include(c => c.Category)
                .Include(c => c.CourseContents)
                .Include(c => c.CourseSkills)
                .Include(c => c.TargetLearners)
                .Where(c => c.Id == id)
                .FirstAsync();
                //.Include(c => c.Enrollments) 

            if (course == null) throw new NotFoundException("Khóa học không tồn tại.");

            return new CourseDetailDto
            {
                Id = course.Id,
                Title = course.Title,
                Description = course.Description,
                Price = course.Price,
                Level = course.Level,
                ImageUrl = course.ImageUrl,
                TeacherName = course.TeacherName,
                DurationHours = course.DurationHours,
                CategoryName = course.Category?.Name ?? "Uncategorized",
                AverageRating = course.AverageRating,
                TotalPurchased = course.TotalPurchased,
                UpdatedAt = course.UpdatedAt,
                CourseContents = course.CourseContents.Select(c => new CourseContentDto { Title = c.Title, Description = c.Description }).ToList(),
                CourseSkills = course.CourseSkills.Select(s => new SkillTargetDto { Description = s.Name }).ToList(),
                TargetLearners = course.TargetLearners.Select(t => new TargetLearnerDto { Description = t.Description }).ToList()
            };
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


    public async Task<CourseDetailDto> UpdateCourseAsync(int courseId, int sellerId, UpdateCourseDto request)
            {
                // 1. Lấy khóa học kèm theo các bảng con (Include) để update
                var course = await _context.Courses
                    .Include(c => c.CourseContents)
                    .Include(c => c.CourseSkills)
                    .Include(c => c.TargetLearners)
                    .Include(c => c.Category)
                    .FirstOrDefaultAsync(c => c.Id == courseId);

                if (course == null)
                {
                    throw new NotFoundException($"Không tìm thấy khóa học với ID: {courseId}");
                }

                // 2. Kiểm tra quyền sở hữu
                if (course.SellerId != sellerId)
                {
                    throw new UnauthorizedException("Bạn không có quyền chỉnh sửa khóa học này.");
                }

                // 3. Cập nhật các thông tin cơ bản
                course.Title = request.Title ?? course.Title;
                course.Description = request.Description ?? "";
                course.Price = request.Price ?? course.Price;
                course.Level = request.Level?? course.Level;
                course.DurationHours = request.DurationHours ?? course.DurationHours    ;
                course.TeacherName = request.TeacherName ?? course.TeacherName;
                course.UpdatedAt = DateTime.UtcNow;

                // Cập nhật Category (Giả sử logic là đổi tên category hiện tại hoặc gán category mới)
       
                course.CategoryId = request.CategoryId ?? course.CategoryId;

                // 4. Cập nhật danh sách con (Strategy: Xóa hết cũ, thêm mới)
                // Xóa CourseContents cũ
                _context.CourseContents.RemoveRange(course.CourseContents);
                // Thêm mới từ request
                course.CourseContents = request.CourseContents?.Select(cc => new CourseContent
                {
                    Title = cc.Title,
                    Description = cc.Description ?? "",
                    CourseId = courseId // Gán FK tường minh
                }).ToList() ?? new List<CourseContent>();

                // Xóa Skills cũ & Thêm mới
                _context.CourseSkills.RemoveRange(course.CourseSkills);
                course.CourseSkills = request.CourseSkills?.Select(cs => new CourseSkill
                {
                    Name = cs.Description,
                    CourseId = courseId
                }).ToList() ?? new List<CourseSkill>();

                // Xóa TargetLearners cũ & Thêm mới
                _context.TargetLearners.RemoveRange(course.TargetLearners);
                course.TargetLearners = request.TargetLearners?.Select(tl => new TargetLearner
                {
                    Description = tl.Description,
                    CourseId = courseId
                }).ToList() ?? new List<TargetLearner>();

                // 5. Lưu xuống DB
                await _context.SaveChangesAsync();

                // 6. Trả về DTO chi tiết sau khi update (Gọi lại hàm GetById để map dữ liệu chuẩn nhất)
                return await GetCourseByIdAsync(courseId);
            }
        }

}
