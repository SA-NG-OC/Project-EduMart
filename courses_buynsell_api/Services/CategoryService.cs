using courses_buynsell_api.DTOs.Category;
using courses_buynsell_api.Data;
using courses_buynsell_api.Interfaces;
using Microsoft.EntityFrameworkCore;
namespace courses_buynsell_api.Services;

public class CategoryService : ICategoryService
{
    private readonly AppDbContext _context;
    public CategoryService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CategoryResponseDto>> GetAllCategoriesAsync()
    {
        try
        {
            var categories = await _context.Categories.ToListAsync();
            var categoryDtos = categories.Select(c => new CategoryResponseDto
            {
                Id = c.Id,
                Name = c.Name,
                CreatedAt = c.CreatedAt
            });

            return categoryDtos;
        }
        catch (Exception ex)
        {
            // Log the exception (you can use any logging framework)
            throw new Exception("An error occurred while retrieving categories.", ex);
        }
    }

    public async Task AddCategoryAsync(AddCategoryRequestDto dto)
    {
        var category = new Entities.Category
        {
            Name = dto.Name
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateCategoryAsync(UpdateCategoryRequestDto dto)
    {
        var category = await _context.Categories.FindAsync(dto.Id);
        if (category == null)
        {
            throw new Exception("Category not found.");
        }

        category.Name = dto.Name;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteCategoryAsync(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null)
        {
            throw new Exception("Category not found.");
        }

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
    }

}