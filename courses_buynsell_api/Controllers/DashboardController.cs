using Microsoft.AspNetCore.Mvc;
using courses_buynsell_api.Interfaces;
using courses_buynsell_api.DTOs.Dashboard;
using Microsoft.AspNetCore.Authorization;

namespace courses_buynsell_api.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _service;

    public DashboardController(IDashboardService service)
    {
        _service = service;
    }

    // Thống kê theo danh mục của admin
    [HttpGet("course/category")]
    [Authorize(Roles = "Admin, Seller")]
    public async Task<ActionResult<List<CategoryCourseCountDto>>> GetCourseCountByCategory()
    {
        var result = await _service.GetCourseCountByCategoryAsync();
        return Ok(result);
    }

    // Thống kê số lượt giao dịch hàng tháng của seller
    [HttpGet("monthly/buyer")]
    [Authorize(Roles = "Seller")]
    public async Task<ActionResult<List<MonthlyBuyerStatsDto>>> GetMonthlyBuyerStats()
    {
        int sellerId = HttpContext.Items["UserId"] as int? ?? -1;
        if (sellerId == -1)
        {
            return Unauthorized("You are not authorized to access this resource.");
        }
        var result = await _service.GetMonthlyBuyerStatsAsync(sellerId);
        return Ok(result);
    }

    //Thống kê theo danh mục của seller
    [HttpGet("category/course")]
    [Authorize(Roles = "Seller")]
    public async Task<ActionResult<List<SellerCategoryCourseCountDto>>> GetCourseCountBySeller()
    {
        int sellerId = HttpContext.Items["UserId"] as int? ?? -1;
        if (sellerId == -1)
        {
            return Unauthorized("You are not authorized to access this resource.");
        }
        var result = await _service.GetCourseCountBySellerAsync(sellerId);
        return Ok(result);
    }

    // Thống kê doanh thu 12 tháng gần nhất của admin
    [HttpGet("stats/revenue/last-12-months")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetLast12MonthsRevenue()
    {
        var data = await _service.GetLast12MonthsRevenueAsync();
        return Ok(data);
    }

    // Thống kê doanh thu 12 tháng gần nhất của seller
    [HttpGet("seller/revenue")]
    [Authorize(Roles = "Seller")]
    public async Task<ActionResult<List<MonthlyRevenueDto>>> GetSellerMonthlyRevenue(int sellerId)
    {
        var result = await _service.GetSellerMonthlyRevenueAsync(sellerId);
        return Ok(result);
    }

    // Thống kê theo role của admin
    [HttpGet("role-count")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<UserRoleCountDto>>> GetUserRoleCount()
    {
        var result = await _service.GetUserRoleCountAsync();
        return Ok(result);
    }

    // Thông kê các khóa học bán chạy.
    // Thông kê các giao dịch gần đây 
    [HttpGet("recent")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<RecentTransactionDto>>> GetRecentTransactions()
    {
        var result = await _service.GetRecentTransactionsAsync();
        return Ok(result);
    }

    [HttpGet("total-revenue")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetTotalRevenue()
    {
        var result = await _service.GetTotalRevenueAsync();
        return Ok(result);
    }

    [HttpGet("seller/total-revenue")]
    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> GetSellerRevenue()
    {
        int sellerId = HttpContext.Items["UserId"] as int? ?? -1;
        if (sellerId == -1)
            return Unauthorized("You are not authorized to access this resource.");

        var result = await _service.GetRevenueBySellerAsync(sellerId);
        return Ok(result);
    }

    [HttpGet("seller/stats")]
    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> GetSellerStats()
    {
        int sellerId = HttpContext.Items["UserId"] as int? ?? -1;
        if (sellerId == -1)
            return Unauthorized("You are not authorized to access this resource.");

        var result = await _service.GetSellerStatsAsync(sellerId);
        return Ok(result);
    }

    [HttpGet("user")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetUserStatistics()
    {
        var result = await _service.GetUserStatisticsAsync();
        return Ok(result);
    }

    [HttpGet("course/{courseId}/monthly-revenue")]
    [Authorize(Roles = "Seller")]
    public async Task<ActionResult<List<MonthlyRevenueDto>>> GetCourseMonthlyRevenue(int courseId)
    {
        int sellerId = HttpContext.Items["UserId"] as int? ?? -1;
        if (sellerId == -1)
        {
            return Unauthorized("You are not authorized to access this resource.");
        }
        var data = await _service.GetMonthlyRevenueByCourseAsync(sellerId, courseId);
        return Ok(data);
    }

    [HttpGet("courses/{courseId}/review-stars")]
    [Authorize(Roles = "Seller")]
    public async Task<IActionResult> GetReviewStarCountsByCourse(int courseId)
    {
        int sellerId = HttpContext.Items["UserId"] as int? ?? -1;
        if (sellerId == -1)
            return Unauthorized("You are not authorized to access this resource.");

        var data = await _service.GetReviewStarCountsByCourseAsync(sellerId, courseId);
        return Ok(data);
    }

    [HttpGet("revenue-7-days")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetRevenueLast7Days()
    {
        var result = await _service.GetRevenueLast7DaysAsync();
        return Ok(result);
    }

}
