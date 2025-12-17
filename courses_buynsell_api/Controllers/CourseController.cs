using courses_buynsell_api.DTOs.Course; 
using courses_buynsell_api.Exceptions;  
using courses_buynsell_api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace courses_buynsell_api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("/[controller]")]
    public class CourseController : ControllerBase
    {
        private readonly ICourseService _courseService;

        public CourseController(ICourseService courseService)
        {
            _courseService = courseService;
        }

        // GET: /Course
        // Cho phép xem danh sách khóa học mà không cần đăng nhập
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetAllCourses(
            [FromQuery] string? keyword,
            [FromQuery] int? categoryId,
            [FromQuery] string? level,
            [FromQuery] bool? isApproved,
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _courseService.GetAllCoursesAsync(keyword, categoryId, level, isApproved, minPrice, maxPrice, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // GET: /Course/{id}
        // Xem chi tiết khóa học
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCourseById(int id)
        {
            try
            {
                var course = await _courseService.GetCourseByIdAsync(id);
                return Ok(course);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // POST: /Course
        // Người dùng tạo khóa học 
        [Authorize(Roles = "Admin, Seller")]
        [HttpPost]
        public async Task<IActionResult> CreateCourse([FromForm] CreateCourseDto request)
        {
            try
            {
                int userId = HttpContext.Items["UserId"] as int? ?? -1;
                if (userId == -1)
                {
                    return Unauthorized(new { message = "Không xác định được người dùng hiện tại." });
                }

                var newCourse = await _courseService.CreateCourseAsync(userId, request);
                // Trả về 201 Created
                return StatusCode(201, newCourse);
            }
            catch (BadRequestException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // PUT: /Course/{id}
        // Cập nhật khóa học (Chỉ Seller tạo ra nó mới được)
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCourse(int id, [FromForm] UpdateCourseDto request)
        {
            try
            {
                int userId = HttpContext.Items["UserId"] as int? ?? -1;

                // Gọi service, service sẽ check xem userId này có phải chủ khóa học không
                var updatedCourse = await _courseService.UpdateCourseAsync(id, userId, request);
                return Ok(updatedCourse);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedException ex) // Bắt lỗi nếu user không phải chủ khóa học
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (BadRequestException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // DELETE: /Course/{id}
        // Xóa khóa học (Chính chủ Seller mới được xóa)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            try
            {
                int userId = HttpContext.Items["UserId"] as int? ?? -1;

                // Gọi service để xóa
                await _courseService.DeleteCourseAsync(id, userId);

                // Trả về 204 No Content khi xóa thành công
                return NoContent();
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedException ex) // Bắt lỗi nếu user xóa khóa học của người khác
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // PUT: /Course/{id}/Approve
        // Admin duyệt khóa học
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}/Approve")]
        public async Task<IActionResult> ApproveCourse(int id)
        {
            try
            {
                await _courseService.ApproveCourseAsync(id);
                return Ok(new { message = "Đã duyệt khóa học thành công." });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // GET: /Course/MyCourses
        // Lấy danh sách khóa học do user hiện tại tạo (Dashboard của Seller)
        [HttpGet("MyCourses")]
        public async Task<IActionResult> GetMyCourses(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                int userId = HttpContext.Items["UserId"] as int? ?? -1;
                var result = await _courseService.GetCoursesBySellerIdAsync(userId, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}