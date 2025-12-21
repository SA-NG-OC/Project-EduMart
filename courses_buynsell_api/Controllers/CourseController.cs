using courses_buynsell_api.DTOs.Course;
using courses_buynsell_api.Exceptions;
using courses_buynsell_api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace courses_buynsell_api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CourseController : ControllerBase
    {
        private readonly ICourseService _courseService;

        public CourseController(ICourseService courseService)
        {
            _courseService = courseService;
        }

        // GET: api/Course/all
        // Cho phép xem danh sách khóa học mà không cần đăng nhập
        [HttpGet("all")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAnonymously([FromQuery] CourseQueryParameters queryParameters)
        {
            if (((queryParameters.IncludeRestricted ?? false) || (queryParameters.IncludeUnapproved ?? false)))
                return BadRequest();
            var result = await _courseService.GetCoursesAsync(queryParameters);
            return Ok(result);
        }

        [HttpGet]
        [Authorize(Roles = "Admin, Buyer, Seller")]
        public async Task<IActionResult> Get([FromQuery] CourseQueryParameters queryParameters)
        {
            if (((queryParameters.IncludeRestricted ?? false) || (queryParameters.IncludeUnapproved ?? false))
                && User.IsInRole("Buyer"))
                return BadRequest("Buyer can not get restricted or unapproved courses");
            var result = await _courseService.GetCoursesAsync(queryParameters);
            return Ok(result);
        }

        // GET: api/Course/{id}
        // Xem chi tiết khóa học
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCourseById(int id)
        {
            try
            {
                int userId = 0;
                var Id = User.FindFirst("id")?.Value;
                if (Id != null)
                {
                    userId = int.Parse(Id);
                }

                var course = await _courseService.GetByIdAsync(id, userId);
                if (course == null) return NotFound();
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

        // POST: api/Course
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

                var newCourse = await _courseService.CreateAsync(request, userId);
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

        // PUT: api/Course/{id}
        // Cập nhật khóa học (Chỉ Seller tạo ra nó mới được)
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCourse(int id, [FromForm] UpdateCourseDto request)
        {
            try
            {
                int userId = int.Parse(User.FindFirst("id")!.Value);

                var updated = await _courseService.UpdateAsync(id, request, userId);
                if (updated == null) return NotFound();
                return Ok(updated);
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

        // DELETE: api/Course/{id}
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

        // PUT: api/Course/{courseId}/approve
        // Admin duyệt khóa học
        [Authorize(Roles = "Admin")]
        [HttpPut("{courseId:int}/approve")]
        public async Task<IActionResult> ApproveCourse(int courseId)
        {
            try
            {
                await _courseService.ApproveCourseAsync(courseId);
                return NoContent();
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

        // GET: api/Course/
        // Lấy danh sách khóa học do user hiện tại tạo (Dashboard của Seller)
        [HttpGet("Course")]
        [Authorize(Roles = "Seller")]
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