namespace courses_buynsell_api.Interfaces;

using courses_buynsell_api.DTOs.Dashboard;
public interface IDashboardService
{
    Task<List<MonthlyBuyerStatsDto>> GetMonthlyBuyerStatsAsync(int sellerId);
    Task<List<CategoryCourseCountDto>> GetCourseCountByCategoryAsync();
    Task<List<SellerCategoryCourseCountDto>> GetCourseCountBySellerAsync(int sellerId);
    Task<List<MonthlyRevenueDto>> GetLast12MonthsRevenueAsync();
    Task<List<MonthlyRevenueDto>> GetSellerMonthlyRevenueAsync(int sellerId);
    Task<List<UserRoleCountDto>> GetUserRoleCountAsync();
    Task<List<RecentTransactionDto>> GetRecentTransactionsAsync(int count = 5);
    Task<RevenueDto> GetTotalRevenueAsync();
    Task<RevenueDto> GetRevenueBySellerAsync(int sellerId);
    Task<SellerStatsDto> GetSellerStatsAsync(int sellerId);
    Task<UserStatisticsDto> GetUserStatisticsAsync();
    Task<List<MonthlyRevenueDto>> GetMonthlyRevenueByCourseAsync(int sellerId, int courseId);
    Task<List<ReviewStarCountDto>> GetReviewStarCountsByCourseAsync(int sellerId, int courseId);
    Task<List<DailyRevenueDto>> GetRevenueLast7DaysAsync();

}