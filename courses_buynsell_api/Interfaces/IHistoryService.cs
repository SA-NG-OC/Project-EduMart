using courses_buynsell_api.DTOs; // Import namespace chứa PagedResult
using courses_buynsell_api.DTOs.History;

namespace courses_buynsell_api.Interfaces;

public interface IHistoryService
{
    // Sửa dòng này: Thêm page, pageSize và đổi kiểu trả về
    Task<PagedResult<HistoryItemDto>> GetHistoryAsync(int userId, int page, int pageSize);

    Task AddToHistoryAsync(int userId, int courseId);
    Task RemoveFromHistoryAsync(int userId, int courseId);
    Task ClearHistoryAsync(int userId);
}