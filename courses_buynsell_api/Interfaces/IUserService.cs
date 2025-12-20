using courses_buynsell_api.DTOs.User;
using courses_buynsell_api.Entities;
using courses_buynsell_api.DTOs;
using courses_buynsell_api.DTOs.Course;

namespace courses_buynsell_api.Interfaces;

public interface IUserService
{
    Task<PagedResult<UserListDto>> GetAllUsersAsync(int page, int pageSize);
    Task<UserDetailDto> GetUserByIdAsync(int id);
    Task DeleteUserAsync(DeleteUserRequest request);
    Task<UserDetailDto> UpdateUserAsync(int id, UpdateUserRequest request);
    Task<UserDetailDto> AddAdminAsync(AddAdminRequest request);
    Task ChangeUserPasswordAsync(ChangeUserPasswordRequest request, int userId);
    Task<PagedResult<UserListDto>> GetUsersByRoleAsync(string role, int page, int pageSize);
    Task<UserStatisticsDto> GetUserStatisticsAsync();
    Task<PagedResult<CourseListItemUserDto>> GetMyCourses(CourseQueryParameters q, int userId);
}
