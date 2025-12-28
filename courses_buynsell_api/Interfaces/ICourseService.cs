using courses_buynsell_api.DTOs.Course; // Namespace chứa các DTO của Course
using courses_buynsell_api.DTOs;
using courses_buynsell_api.Entities; // Namespace chứa PagedResult

namespace courses_buynsell_api.Interfaces
{
    public interface ICourseService
    {
        // Lấy danh sách khóa học (Public) có phân trang và bộ lọc tìm kiếm
        Task<PagedResult<CourseListItemDto>> GetCoursesAsync(CourseQueryParameters query);

        // Lấy chi tiết một khóa học
        Task<CourseDetailDto?> GetByIdAsync(int id, int userId);

        // Tạo khóa học mới (cần ID người tạo)
        Task<CourseDetailDto> CreateAsync(CreateCourseDto dto, int userId);

        // Cập nhật khóa học (cần ID khóa học, ID người sửa để check quyền, và data sửa)
        Task<CourseDetailDto?> UpdateAsync(int id, UpdateCourseDto dto, int sellerId);

        // Xóa khóa học
        Task DeleteCourseAsync(int courseId, int userId);

        // Duyệt khóa học (Dành cho Admin)
        Task ApproveCourseAsync(int courseId);

        // Lấy danh sách khóa học của chính người bán (My Courses)
        Task<PagedResult<CourseListItemDto>> GetCoursesBySellerIdAsync(int sellerId, int page, int pageSize);

        // Quản lý Nội dung 
        Task AddCourseContentAsync(int courseId, int userId, CourseContentDto input);
        Task DeleteCourseContentAsync(int courseId, int contentId, int userId);

        // Quản lý Kỹ năng 
        Task AddCourseSkillAsync(int courseId, int userId, SkillTargetDto input);
        Task DeleteCourseSkillAsync(int courseId, int skillId, int userId);

        // Quản lý Đối tượng học viên 
        Task AddTargetLearnerAsync(int courseId, int userId, SkillTargetDto input);
        Task DeleteTargetLearnerAsync(int courseId, int learnerId, int userId);

        // Khóa học đã mua 
        Task<PagedResult<CourseListItemUserDto>> GetPurchasedCoursesAsync(int userId, CourseQueryParameters query);

        // Hạn chế/Bỏ hạn chế 
        Task ToggleRestrictionAsync(int courseId);

        // Quản lý Link học 
        Task<string> GetStudyLinkAsync(int courseId, int userId, string userRole);
        Task UpdateStudyLinkAsync(int courseId, int userId, string? newUrl);
    }
}