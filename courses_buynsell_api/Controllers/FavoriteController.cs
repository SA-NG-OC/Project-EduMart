using courses_buynsell_api.DTOs.Favorite;
using courses_buynsell_api.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
namespace courses_buynsell_api.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class FavoriteController : ControllerBase
{
    private readonly IFavoriteService _favoriteService;

    public FavoriteController(IFavoriteService favoriteService)
    {
        _favoriteService = favoriteService;
    }

    [HttpGet]
    [Authorize(Roles = "Buyer")]
    public async Task<IActionResult> GetFavorites()
    {
        try
        {
            int userId = HttpContext.Items["UserId"] as int? ?? -1;
            if (userId == -1)
            {
                return Unauthorized(new { message = "User not authenticated." });
            }
            var result = await _favoriteService.GetFavoritesByUserIdAsync(userId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpPost("{courseId}")]
    [Authorize(Roles = "Buyer")]
    public async Task<IActionResult> AddFavorite(int courseId)
    {
        try
        {
            int userId = HttpContext.Items["UserId"] as int? ?? -1;
            if (userId == -1)
            {
                return Unauthorized(new { message = "User not authenticated." });
            }

            var success = await _favoriteService.AddFavoriteAsync(userId, courseId);
            if (!success)
            {
                return Conflict(new { message = "Favorite already exists." });
            }

            return StatusCode(201);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [Authorize(Roles = "Buyer")]
    [HttpDelete("clear")]
    public async Task<IActionResult> ClearFavorites()
    {
        int userId = HttpContext.Items["UserId"] as int? ?? -1;
        if (userId == -1)
        {
            return Unauthorized(new { message = "User not authenticated." });
        }

        var result = await _favoriteService.ClearFavoritesAsync(userId);

        if (!result)
            return NotFound(new { message = "No favorites to delete." });

        return StatusCode(204);
    }

    [Authorize(Roles = "Buyer")]
    [HttpDelete("{courseId}")]
    public async Task<IActionResult> RemoveFavorite(int courseId)
    {
        int userId = HttpContext.Items["UserId"] as int? ?? -1;
        if (userId == -1)
        {
            return Unauthorized(new { message = "User not authenticated." });
        }

        var result = await _favoriteService.RemoveFavoriteAsync(userId, courseId);

        if (!result)
            return NotFound(new { message = "Favorite not found for this course." });

        return Ok(new { message = "Course removed from favorites." });
    }

}
