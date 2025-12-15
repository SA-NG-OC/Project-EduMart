using courses_buynsell_api.DTOs.Course; // Namespace chứa các DTO của Course
using courses_buynsell_api.DTOs;
using courses_buynsell_api.Entities; // Namespace chứa PagedResult

namespace courses_buynsell_api.Interfaces
{
    public interface ICourseService
    {
        // Lấy danh sách khóa học (Public) có phân trang và bộ lọc tìm kiếm
        Task<PagedResult<CourseListItemDto>> GetAllCoursesAsync(
            string? keyword,
            int? categoryId,
            string? level,       
            bool? isApproved,
            decimal? minPrice,
            decimal? maxPrice,
            int page,
            int pageSize
        );

        // Lấy chi tiết một khóa học
        Task<CourseDetailDto> GetCourseByIdAsync(int id);

        // Tạo khóa học mới (cần ID người tạo)
        Task<CourseDetailDto> CreateCourseAsync(int sellerId, CreateCourseDto request);

        // Cập nhật khóa học (cần ID khóa học, ID người sửa để check quyền, và data sửa)
        Task<CourseDetailDto> UpdateCourseAsync(int courseId, int sellerId, UpdateCourseDto request);

        // Xóa khóa học
        Task DeleteCourseAsync(int courseId, int userId);

        // Duyệt khóa học (Dành cho Admin)
        Task ApproveCourseAsync(int courseId);

        // Lấy danh sách khóa học của chính người bán (My Courses)
        Task<PagedResult<CourseListItemDto>> GetCoursesBySellerIdAsync(int sellerId, int page, int pageSize);
    }
}