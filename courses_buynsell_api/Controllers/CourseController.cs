using courses_buynsell_api.DTOs.Course;
using courses_buynsell_api.Exceptions;
using courses_buynsell_api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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

        // POST: api/Course/{courseId}/contents
        // Thêm nội dung khóa học
        [HttpPost("{courseId}/contents")]
        public async Task<IActionResult> AddContent(int courseId, [FromBody] CourseContentDto input)
        {
            try
            {
                int userId = int.Parse(User.FindFirst("id")!.Value);
                await _courseService.AddCourseContentAsync(courseId, userId, input);
                return Ok();
            }
            catch (NotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (UnauthorizedException ex) { return Unauthorized(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // DELETE: api/Course/{courseId}/contents/{contentId}
        // Xóa nội dung khóa học
        [HttpDelete("{courseId}/contents/{contentId}")]
        public async Task<IActionResult> DeleteContent(int courseId, int contentId)
        {
            try
            {
                int userId = int.Parse(User.FindFirst("id")!.Value);
                await _courseService.DeleteCourseContentAsync(courseId, contentId, userId);
                return NoContent(); // 204
            }
            catch (NotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (UnauthorizedException ex) { return Unauthorized(new { message = ex.Message }); }
            catch (BadRequestException ex) { return BadRequest(new { message = ex.Message }); } // Content ko thuộc Course
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // POST: api/Course/{courseId}/skills
        // Thêm kỹ năng cho khóa học
        [HttpPost("{courseId}/skills")]
        public async Task<IActionResult> AddSkill(int courseId, [FromBody] SkillTargetDto input)
        {
            try
            {
                int userId = int.Parse(User.FindFirst("id")!.Value);
                await _courseService.AddCourseSkillAsync(courseId, userId, input);
                return Ok();
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // DELETE: api/Course/{courseId}/skills/{skillId}
        // Xóa kỹ năng khỏi khóa học
        [HttpDelete("{courseId}/skills/{skillId}")]
        public async Task<IActionResult> DeleteSkill(int courseId, int skillId)
        {
            try
            {
                int userId = int.Parse(User.FindFirst("id")!.Value);
                await _courseService.DeleteCourseSkillAsync(courseId, skillId, userId);
                return NoContent();
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        //  POST: api/Course/{courseId}/target-learners
        // Thêm đối tượng học viên cho khóa học
        [HttpPost("{courseId}/target-learners")]
        public async Task<IActionResult> AddTargetLearner(int courseId, [FromBody] SkillTargetDto input)
        {
            try
            {
                int userId = int.Parse(User.FindFirst("id")!.Value);
                await _courseService.AddTargetLearnerAsync(courseId, userId, input);
                return Ok();
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // DELETE: api/Course/{courseId}/target-learners/{learnerId}
        // Xóa đối tượng học viên khỏi khóa học
        [HttpDelete("{courseId}/target-learners/{learnerId}")]
        public async Task<IActionResult> DeleteTargetLearner(int courseId, int learnerId)
        {
            try
            {
                int userId = int.Parse(User.FindFirst("id")!.Value);
                await _courseService.DeleteTargetLearnerAsync(courseId, learnerId, userId);
                return NoContent();
            }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // PUT: api/Course/{courseId}/restrict
        // Hạn chế/Bỏ hạn chế khóa học 
        [HttpPut("{courseId}/restrict")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ToggleRestrict(int courseId)
        {
            try
            {
                await _courseService.ToggleRestrictionAsync(courseId);
                return Ok(new { message = "Restriction status updated" });
            }
            catch (NotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // GET: api/Course/{courseId}/study
        // Lấy link học tập cho khóa học đã mua
        [HttpGet("{courseId}/study")]
        public async Task<IActionResult> GetStudyLink(int courseId)
        {
            try
            {
                int userId = int.Parse(User.FindFirst("id")!.Value);
                var role = User.FindFirst(ClaimTypes.Role)?.Value ?? "";

                var link = await _courseService.GetStudyLinkAsync(courseId, userId, role);

                if (link == "No lecture found") return Ok("No lecture found"); 

                return Ok(new { url = link });
            }
            catch (UnauthorizedException ex) { return Unauthorized(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }

        // PUT: api/Course/{courseId}/study
        // Cập nhật link học tập cho khóa học đã mua
        [HttpPut("{courseId}/study")]
        public async Task<IActionResult> UpdateStudyLink(int courseId, [FromBody] StudyLinkDto? input)
        {
            try
            {
                int userId = int.Parse(User.FindFirst("id")!.Value);
                await _courseService.UpdateStudyLinkAsync(courseId, userId, input?.Url);
                return Ok();
            }
            catch (UnauthorizedException ex) { return Unauthorized(new { message = ex.Message }); } 
            catch (NotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message }); }
        }
    }
}