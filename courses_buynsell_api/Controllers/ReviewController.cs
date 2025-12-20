using Microsoft.AspNetCore.Mvc;
using courses_buynsell_api.Interfaces;
using courses_buynsell_api.DTOs.Review;
using courses_buynsell_api.Exceptions;
using Microsoft.AspNetCore.Authorization;

namespace courses_buynsell_api.Controllers
{
    [ApiController]
    [Route("/[controller]")]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        /// <summary>
        /// Lấy danh sách review theo CourseId
        /// </summary>
        [HttpGet("Course/{courseId}")]
        public async Task<IActionResult> GetByCourseId(int courseId)
        {
            try
            {
                var reviews = await _reviewService.GetByCourseId(courseId);
                return Ok(reviews);
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

        /// <summary>
        /// Lấy danh sách review của khóa học thuộc seller cụ thể
        /// </summary>
        [Authorize(Roles = "Seller")]
        [HttpGet("Seller/Course/{courseId}")]
        public async Task<IActionResult> GetBySellerCourses(int courseId)
        {
            try
            {
                int sellerId = HttpContext.Items["UserId"] as int? ?? -1;
                var reviews = await _reviewService.GetBySellerCourses(courseId, sellerId);
                return Ok(reviews);
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

        // Xóa comment review của Admin
        [Authorize(Roles = "Admin")]
        [HttpDelete("{reviewId}")]
        public async Task<IActionResult> DeleteReview(int reviewId)
        {
            try
            {
                await _reviewService.DeleteReview(reviewId);
                return NoContent();
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ForbiddenException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // Xóa comment review của Buyer
        [HttpDelete("User/{reviewId}")]
        [Authorize(Roles = "Buyer, Admin")]
        public async Task<IActionResult> DeleteReviewByUser(int reviewId)
        {
            try
            {
                int userId = HttpContext.Items["UserId"] as int? ?? -1;
                await _reviewService.DeleteReviewByUser(userId, reviewId);
                return NoContent();
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ForbiddenException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // Tạo mới review
        [Authorize(Roles = "Buyer, Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateReview([FromBody] ReviewRequestDto reviewDto)
        {
            try
            {
                int buyerId = HttpContext.Items["UserId"] as int? ?? -1;
                await _reviewService.CreateReview(reviewDto, buyerId);
                return StatusCode(201);
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

        // Update review
        [HttpPut("{reviewId}")]
        [Authorize(Roles = "Buyer, Admin")]
        public async Task<IActionResult> UpdateReview(int reviewId, [FromBody] ReviewUpdateRequest reviewUpdateDto)
        {
            try
            {
                int userId = HttpContext.Items["UserId"] as int? ?? -1;
                await _reviewService.UpdateReview(reviewId, userId, reviewUpdateDto);
                return NoContent();
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ForbiddenException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
