using courses_buynsell_api.DTOs.Block;
using courses_buynsell_api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace courses_buynsell_api.Controllers;

[Route("[controller]")]
[ApiController]
[Authorize]
public class BlockController : ControllerBase
{
    private readonly IBlockService _blockService;

    public BlockController(IBlockService blockService)
    {
        _blockService = blockService;
    }

    private int GetUserId()
    {
        int id = HttpContext.Items["UserId"] as int? ?? -1;
        if (id == -1)
        {
            throw new UnauthorizedAccessException("Không xác định được người dùng hiện tại.");
        }
        return id;
    }

    [HttpPost]
    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> BlockUser([FromBody] BlockUserRequest request)
    {
        // Lấy ID của người đang đăng nhập (Seller) từ Token
        // Lưu ý: Logic lấy Id này tùy thuộc vào cách bạn cấu hình Authen
        var userIdString = GetUserId().ToString();

        if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int sellerId))
        {
            return Unauthorized("Không xác định được người dùng.");
        }

        var result = await _blockService.BlockUserAsync(sellerId, request.UserToBlockId);

        if (result)
        {
            return Ok(new { message = "Đã chặn người dùng thành công." });
        }

        return BadRequest(new { message = "Không thể chặn người dùng này (hoặc lỗi hệ thống)." });
    }

    [HttpDelete("{userId}")]
    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> UnblockUser(int userId)
    {
        var currentUserId = GetUserId();

        if (currentUserId == 0) return Unauthorized();

        try
        {
            var result = await _blockService.UnblockUserAsync(currentUserId, userId);
            if (result)
            {
                return Ok(new { message = "User unblocked successfully" });
            }
            return BadRequest(new { message = "User was not blocked or error occurred" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpGet("check/{targetUserId}")]
    public async Task<IActionResult> CheckBlockStatus(int targetUserId)
    {
        try
        {
            var currentUserId = GetUserId();

            // Gọi service kiểm tra 2 chiều (A chặn B hoặc B chặn A)
            var isBlocked = await _blockService.IsBlockedAsync(currentUserId, targetUserId);

            return Ok(new
            {
                targetUserId = targetUserId,
                isBlocked = isBlocked
            });
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { message = "Vui lòng đăng nhập." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi server: " + ex.Message });
        }
    }
}