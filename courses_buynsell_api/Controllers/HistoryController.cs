using courses_buynsell_api.Exceptions;
using courses_buynsell_api.Interfaces;
using courses_buynsell_api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace courses_buynsell_api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")] 
    public class HistoryController : ControllerBase
    {
        private readonly IHistoryService _historyService;

        public HistoryController(IHistoryService historyService)
        {
            _historyService = historyService;
        }

        // GET: /History?page=1&pageSize=10
        [HttpGet]
        public async Task<IActionResult> GetHistory(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                int userId = int.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("id")?.Value ?? "-1");
                var result = await _historyService.GetHistoryAsync(userId, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // POST: /History/{courseId}
        [HttpPost("{courseId}")]
        public async Task<IActionResult> AddToHistory(int courseId)
        {
            try
            {
                int userId = int.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("id")?.Value ?? "-1");
                await _historyService.AddToHistoryAsync(userId, courseId);

                // Trả về 201 Created 
                return StatusCode(201, new { message = "Đã thêm vào lịch sử." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // DELETE: /History/{courseId}
        [HttpDelete("{courseId}")]
        public async Task<IActionResult> RemoveItem(int courseId)
        {
            try
            {
                int userId = int.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("id")?.Value ?? "-1");
                await _historyService.RemoveFromHistoryAsync(userId, courseId);
                return NoContent(); // 204
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

        // DELETE: /History/clear
        [HttpDelete("clear")]
        public async Task<IActionResult> ClearAll()
        {
            try
            {
                int userId = int.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("id")?.Value ?? "-1");
                await _historyService.ClearHistoryAsync(userId);
                return NoContent(); // 204
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}