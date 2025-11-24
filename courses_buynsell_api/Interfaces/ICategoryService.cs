using courses_buynsell_api.DTOs.Category;

namespace courses_buynsell_api.Interfaces;

public interface ICategoryService
{
    Task<IEnumerable<CategoryResponseDto>> GetAllCategoriesAsync();
    Task AddCategoryAsync(AddCategoryRequestDto dto);
    Task UpdateCategoryAsync(UpdateCategoryRequestDto dto);
    Task DeleteCategoryAsync(int id);
}