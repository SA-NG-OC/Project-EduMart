using courses_buynsell_api.DTOs.Cart;
using courses_buynsell_api.Exceptions;
using courses_buynsell_api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace courses_buynsell_api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")] 
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        // GET: /api/Cart -> Trả về mảng []
        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            try
            {
                int userId = int.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("id")?.Value ?? "-1");
                var result = await _cartService.GetMyCartDetailsAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // POST: /api/Cart/items/{courseId} 
        [HttpPost("items/{courseId}")]
        public async Task<IActionResult> AddToCart(int courseId)
        {
            try
            {
                int userId = int.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("id")?.Value ?? "-1");
                var result = await _cartService.AddToCartAsync(userId, courseId);
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

        // DELETE: /api/Cart/items/{courseId}
        [HttpDelete("items/{courseId}")]
        public async Task<IActionResult> RemoveFromCart(int courseId)
        {
            try
            {
                int userId = int.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("id")?.Value ?? "-1");
                await _cartService.RemoveFromCartAsync(userId, courseId);
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

        // DELETE: /api/Cart
        [HttpDelete]
        public async Task<IActionResult> ClearCart()
        {
            try
            {
                int userId = int.Parse(User.FindFirst("sub")?.Value ?? User.FindFirst("id")?.Value ?? "-1");
                await _cartService.ClearCartAsync(userId);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}