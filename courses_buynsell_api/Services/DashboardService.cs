namespace courses_buynsell_api.Services;

using courses_buynsell_api.Interfaces;
using Microsoft.EntityFrameworkCore;
using courses_buynsell_api.DTOs.Dashboard;
using courses_buynsell_api.Data;
using courses_buynsell_api.Exceptions;

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _context;

    public DashboardService(AppDbContext context)
    {
        _context = context;
    }

    // Implement dashboard related methods here
    public async Task<List<CategoryCourseCountDto>> GetCourseCountByCategoryAsync()
    {
        return await _context.Categories
            .Select(c => new CategoryCourseCountDto
            {
                CategoryId = c.Id,
                CategoryName = c.Name,
                CourseCount = c.Course.Count
            })
            .ToListAsync();
    }
    public async Task<List<MonthlyBuyerStatsDto>> GetMonthlyBuyerStatsAsync(int sellerId)
    {
        var twelveMonthsAgo = DateTime.UtcNow.AddMonths(-11);

        var stats = await _context.TransactionDetails
            .Where(td => td.Course!.SellerId == sellerId &&
                        td.Transaction!.CreatedAt >= twelveMonthsAgo)
            .GroupBy(td => new
            {
                td.Transaction!.CreatedAt.Year,
                td.Transaction.CreatedAt.Month
            })
            .Select(g => new
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                BuyerCount = g.Count()
            })
            .OrderBy(x => x.Year).ThenBy(x => x.Month)
            .ToListAsync();

        // format sau khi lấy dữ liệu
        return stats.Select(x => new MonthlyBuyerStatsDto
        {
            Month = $"{x.Year}-{x.Month:D2}",
            BuyerCount = x.BuyerCount
        }).ToList();

    }

    public async Task<List<SellerCategoryCourseCountDto>> GetCourseCountBySellerAsync(int sellerId)
    {
        return await _context.Categories
            .Select(c => new SellerCategoryCourseCountDto
            {
                CategoryId = c.Id,
                CategoryName = c.Name,
                CourseCount = c.Course.Count(course => course.SellerId == sellerId)
            })
            .Where(x => x.CourseCount > 0) // nếu muốn hiện cả category 0 course thì bỏ dòng này
            .ToListAsync();
    }

    public async Task<List<MonthlyRevenueDto>> GetLast12MonthsRevenueAsync()
    {
        var now = DateTime.UtcNow;
        var fromMonth = now.AddMonths(-11);

        var startDate = new DateTime(fromMonth.Year, fromMonth.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var revenues = await _context.Transactions
            .Where(t => t.CreatedAt >= startDate)
            .GroupBy(t => new { t.CreatedAt.Year, t.CreatedAt.Month })
            .Select(g => new MonthlyRevenueDto
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                TotalRevenue = g.Sum(x => x.TotalAmount)
            })
            .OrderBy(g => g.Year)
            .ThenBy(g => g.Month)
            .ToListAsync();

        if (!revenues.Any())
            throw new NotFoundException("No revenue data found for the last 12 months.");

        return revenues;
    }

    public async Task<List<MonthlyRevenueDto>> GetSellerMonthlyRevenueAsync(int sellerId)
    {
        var now = DateTime.UtcNow;
        var fromDate = now.AddMonths(-11).AddDays(-now.Day + 1).Date; // đầu tháng cách đây 11 tháng

        var revenueData = await _context.TransactionDetails
            .Where(td => td.Course!.SellerId == sellerId &&
                         td.Transaction!.CreatedAt >= fromDate)
            .GroupBy(td => new
            {
                td.Transaction!.CreatedAt.Year,
                td.Transaction.CreatedAt.Month
            })
            .Select(g => new MonthlyRevenueDto
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                TotalRevenue = g.Sum(x => x.Price)
            })
            .ToListAsync();

        // Bổ sung tháng còn thiếu (trả về 0)
        var result = Enumerable.Range(0, 12)
            .Select(i => now.AddMonths(-i))
            .OrderBy(d => d)
            .Select(d =>
            {
                var match = revenueData.FirstOrDefault(x => x.Year == d.Year && x.Month == d.Month);
                return new MonthlyRevenueDto
                {
                    Year = d.Year,
                    Month = d.Month,
                    TotalRevenue = match?.TotalRevenue ?? 0
                };
            })
            .ToList();

        return result;
    }

    public async Task<List<UserRoleCountDto>> GetUserRoleCountAsync()
    {
        return await _context.Users
            .GroupBy(u => u.Role)
            .Select(g => new UserRoleCountDto
            {
                Role = g.Key,
                Count = g.Count()
            })
            .ToListAsync();
    }

    public async Task<List<RecentTransactionDto>> GetRecentTransactionsAsync(int count = 5)
    {
        return await _context.Transactions
            .OrderByDescending(t => t.CreatedAt)
            .Take(count)
            .Select(t => new RecentTransactionDto
            {
                Id = t.Id,
                TotalAmount = t.TotalAmount,
                BuyerName = t.Buyer!.FullName,
                CreatedAt = t.CreatedAt
            })
            .ToListAsync();
    }

    // Tổng doanh thu toàn hệ thống
    public async Task<RevenueDto> GetTotalRevenueAsync()
    {
        var total = await _context.Transactions
            .SumAsync(t => t.TotalAmount);

        return new RevenueDto { TotalRevenue = total };
    }

    // Tổng doanh thu của seller theo khóa học họ bán
    public async Task<RevenueDto> GetRevenueBySellerAsync(int sellerId)
    {
        var total = await _context.TransactionDetails
            .Where(td => td.Course!.SellerId == sellerId) // SellerId có trong Course
            .SumAsync(td => td.Price);

        return new RevenueDto { TotalRevenue = total };
    }

    // ✅ Thống kê số học viên & rating trung bình
    public async Task<SellerStatsDto> GetSellerStatsAsync(int sellerId)
    {
        var courses = await _context.Courses
            .Where(c => c.SellerId == sellerId)
            .ToListAsync();

        var totalStudents = courses.Sum(c => c.TotalPurchased);

        var avgRating = courses.Any(c => c.AverageRating > 0)
            ? courses.Where(c => c.AverageRating > 0).Average(c => c.AverageRating)
            : 0;


        return new SellerStatsDto
        {
            TotalStudents = totalStudents,
            AverageRating = Math.Round(avgRating, 2)
        };
    }

    public async Task<UserStatisticsDto> GetUserStatisticsAsync()
    {
        var totalUsers = await _context.Users.CountAsync();

        var newUsersToday = await _context.Users
            .CountAsync(u => u.CreatedAt.Date == DateTime.UtcNow.Date);

        return new UserStatisticsDto
        {
            TotalUsers = totalUsers,
            NewUsersToday = newUsersToday
        };
    }

    public async Task<List<MonthlyRevenueDto>> GetMonthlyRevenueByCourseAsync(int sellerId, int courseId)
    {
        if (courseId <= 0)
            throw new ArgumentException("Invalid course ID.", nameof(courseId));

        if (sellerId <= 0)
            throw new UnauthorizedAccessException("Invalid seller ID.");

        // Kiểm tra khóa học có thuộc về người bán hiện tại không
        var ownsCourse = await _context.Courses
            .AnyAsync(c => c.Id == courseId && c.SellerId == sellerId);

        var role = await _context.Users
            .Where(u => u.Id == sellerId)
            .Select(u => u.Role)
            .FirstOrDefaultAsync();

        var isAdmin = role != null && role.Equals("Admin", StringComparison.OrdinalIgnoreCase);

        if (!ownsCourse && role != "Admin")
            throw new UnauthorizedAccessException("You do not have permission to view this course’s revenue.");

        var twelveMonthsAgo = DateTime.UtcNow.AddMonths(-11);

        var query = _context.TransactionDetails
        .Include(td => td.Transaction)
        .Include(td => td.Course)
        .Where(td =>
            td.CourseId == courseId &&
            td.Transaction != null &&
            td.Transaction.CreatedAt >= twelveMonthsAgo
        );

        if (!isAdmin)
        {
            // Seller chỉ thấy của mình
            query = query.Where(td => td.Course!.SellerId == sellerId);
        }

        var result = await query
            .GroupBy(td => new { td.Transaction!.CreatedAt.Year, td.Transaction.CreatedAt.Month })
            .Select(g => new MonthlyRevenueDto
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                TotalRevenue = g.Sum(x => x.Price)
            })
            .OrderBy(g => g.Year)
            .ThenBy(g => g.Month)
            .ToListAsync();

        if (!result.Any())
            throw new NotFoundException($"No revenue data found for course ID {courseId} in the last 12 months.");

        return result;
    }

    public async Task<List<ReviewStarCountDto>> GetReviewStarCountsByCourseAsync(int sellerId, int courseId)
    {
        if (courseId <= 0)
            throw new ArgumentException("Invalid course ID.", nameof(courseId));

        if (sellerId <= 0)
            throw new UnauthorizedAccessException("Invalid seller ID.");

        // Lấy role của user
        var role = await _context.Users
                    .Where(u => u.Id == sellerId)
                    .Select(u => u.Role)
                    .FirstOrDefaultAsync();

        var isAdmin = role == "Admin";

        // Kiểm tra quyền nếu là Seller
        if (!isAdmin)
        {
            var ownsCourse = await _context.Courses
                .AnyAsync(c => c.Id == courseId && c.SellerId == sellerId);

            if (!ownsCourse)
                throw new UnauthorizedAccessException("You do not have permission to view this course’s reviews.");
        }

        // Query cơ bản
        var reviewQuery = _context.Reviews
            .Include(r => r.Course)
            .Where(r => r.CourseId == courseId);

        // Nếu là Seller → chỉ xem review cho course của Seller
        if (!isAdmin)
        {
            reviewQuery = reviewQuery.Where(r => r.Course!.SellerId == sellerId);
        }

        // Group theo sao
        var reviewGroups = await reviewQuery
            .GroupBy(r => r.Star)
            .Select(g => new { Star = g.Key, Count = g.Count() })
            .ToListAsync();

        // Trả đủ 1 → 5 sao
        return Enumerable.Range(1, 5)
            .Select(star => new ReviewStarCountDto
            {
                Star = star,
                Count = reviewGroups.FirstOrDefault(g => g.Star == star)?.Count ?? 0
            })
            .OrderByDescending(x => x.Star)
            .ToList();
    }


    public async Task<List<DailyRevenueDto>> GetRevenueLast7DaysAsync()
    {
        var today = DateTime.UtcNow.Date;
        var startDate = today.AddDays(-6);

        var data = await _context.Transactions
            .Where(t => t.CreatedAt.Date >= startDate && t.CreatedAt.Date <= today)
            .GroupBy(t => t.CreatedAt.Date)
            .Select(g => new DailyRevenueDto
            {
                Date = g.Key,
                Revenue = g.Sum(x => x.TotalAmount)
            })
            .ToListAsync();

        // đảm bảo đủ 7 ngày (kể cả ngày không có doanh thu)
        var result = Enumerable.Range(0, 7)
            .Select(i => startDate.AddDays(i))
            .Select(d => new DailyRevenueDto
            {
                Date = d,
                Revenue = data.FirstOrDefault(x => x.Date == d)?.Revenue ?? 0
            })
            .ToList();

        return result;
    }

}