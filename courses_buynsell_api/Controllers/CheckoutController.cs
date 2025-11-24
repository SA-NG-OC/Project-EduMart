namespace courses_buynsell_api.Controllers;

using courses_buynsell_api.DTOs.Momo;
using courses_buynsell_api.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;


[Authorize]
[ApiController]
[Route("[controller]")]

public class CheckoutController : ControllerBase
{
    private readonly ICheckoutService _checkoutService;

    public CheckoutController(ICheckoutService checkoutService)
    {
        _checkoutService = checkoutService;
    }

    [HttpPost("CreateMomoPayment")]
    public async Task<IActionResult> CreateMomoPayment([FromBody] CreateMomoPaymentRequestDto dto)
    {
        int buyerId = HttpContext.Items["UserId"] as int? ?? -1;
        var payUrl = await _checkoutService.CreateMomoPaymentAsync(dto, buyerId);
        return Ok(new { payUrl });
    }

    [HttpPost("MomoNotify")]
    public async Task<IActionResult> MomoNotify()
    {
        var form = Request.Form.ToDictionary(x => x.Key, x => x.Value.ToString());
        await _checkoutService.HandleMomoCallbackAsync(form);
        return Ok();
    }

    [HttpGet("MomoConfirm")]
    public async Task<IActionResult> Confirm([FromQuery] Dictionary<string, string> queryParams)
    {
        await _checkoutService.HandleMomoCallbackAsync(queryParams);
        return Redirect("http://localhost:5173/payment-success");
    }

}
