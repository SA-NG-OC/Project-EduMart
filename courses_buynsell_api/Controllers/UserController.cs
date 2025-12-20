using courses_buynsell_api.DTOs.Course;
using courses_buynsell_api.Interfaces;
using Microsoft.AspNetCore.Mvc;
using courses_buynsell_api.Exceptions;
using courses_buynsell_api.DTOs.User;
using Microsoft.AspNetCore.Authorization;

namespace courses_buynsell_api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("/[controller]")] // http://localhost:5230/User/Detail
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        // GET: /User
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllUsers(int page = 1, int pageSize = 10)
        {
            try
            {
                var result = await _userService.GetAllUsersAsync(page, pageSize);
                return Ok(result);
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


        // GET: /User/Detail
        [HttpGet("Detail")]
        public async Task<IActionResult> GetUserById()
        {
            try
            {
                int id = HttpContext.Items["UserId"] as int? ?? -1;
                if (id == -1)
                {
                    return Unauthorized(new { message = "Không xác định được người dùng hiện tại." });
                }
                var user = await _userService.GetUserByIdAsync(id);
                return Ok(user);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (BadRequestException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedException ex)
            {
                return Unauthorized("Bạn không có quyền truy cập tài nguyên này." + ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // GET: /User/{id}
        [Authorize(Roles = "Admin, Seller")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserByIdAdmin(int id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                return Ok(user);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
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

        // DELETE: /User
        [Authorize(Roles = "Admin")]
        [HttpDelete]
        public async Task<IActionResult> DeleteUser([FromBody] DeleteUserRequest request)
        {
            try
            {
                int id = HttpContext.Items["UserId"] as int? ?? -1;
                if (id == -1)
                {
                    return Unauthorized(new { message = "Không xác định được người dùng hiện tại." });
                }
                if (id == request.Id)
                {
                    return BadRequest(new { message = "Bạn không thể xóa chính mình." });
                }
                await _userService.DeleteUserAsync(request);
                return NoContent();
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
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

        // PUT: /User/
        [HttpPut]
        public async Task<IActionResult> UpdateUser([FromForm] UpdateUserRequest request)
        {
            try
            {
                int id = HttpContext.Items["UserId"] as int? ?? -1;
                if (id == -1)
                {
                    return Unauthorized(new { message = "Không xác định được người dùng hiện tại." });
                }
                var updatedUser = await _userService.UpdateUserAsync(id, request);
                return Ok(updatedUser);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
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

        // POST: /User/Admin
        [Authorize(Roles = "Admin")]
        [HttpPost("Admin")]
        public async Task<IActionResult> AddAdmin([FromBody] AddAdminRequest request)
        {
            try
            {
                var newAdmin = await _userService.AddAdminAsync(request);
                return Ok(newAdmin);
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

        // PUT: /User/ChangePassword
        [HttpPut("ChangePassword")]
        public async Task<IActionResult> ChangeUserPassword([FromBody] ChangeUserPasswordRequest request)
        {
            try
            {
                int id = HttpContext.Items["UserId"] as int? ?? -1;
                if (id == -1)
                {
                    return Unauthorized(new { message = "Không xác định được người dùng hiện tại." });
                }
                await _userService.ChangeUserPasswordAsync(request, id);
                return NoContent();
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (BadRequestException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("role/{role}")]
        public async Task<IActionResult> GetUsersByRole(string role, int page = 1, int pageSize = 10)
        {
            try
            {
                var result = await _userService.GetUsersByRoleAsync(role, page, pageSize);
                return Ok(result);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
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
        [Authorize(Roles = "Admin")]
        [HttpGet("statistics")]
        public async Task<IActionResult> GetUserStatistics()
        {
            try
            {
                var result = await _userService.GetUserStatisticsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("my-courses")]
        [Authorize(Roles = "Admin, Buyer")]
        public async Task<IActionResult> GetCourses([FromQuery] CourseQueryParameters queryParameters)
        {
            var userId = int.Parse(User.FindFirst("id")!.Value);
            if (((queryParameters.IncludeRestricted ?? false) || (queryParameters.IncludeUnapproved ?? false)))
                return BadRequest();
            var result = await _userService.GetMyCourses(queryParameters, userId);
            return Ok(result);
        }

    }
}