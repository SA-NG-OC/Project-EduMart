using courses_buynsell_api.DTOs;
using courses_buynsell_api.DTOs.Transaction;

namespace courses_buynsell_api.Interfaces
{
    public interface ITransactionService
    {
        Task<PagedResult<TransactionListDto>> GetAllAsync(int page, int pageSize);
        Task<TransactionDetailDto?> GetByCodeAsync(string transactionCode);
        Task<PagedResult<StudentTransactionStatDto>> GetStudentStatsAsync(int page, int pageSize);
        Task<PagedResult<CourseTransactionStatDto>> GetCourseStatsAsync(int page, int pageSize);
    }
}
