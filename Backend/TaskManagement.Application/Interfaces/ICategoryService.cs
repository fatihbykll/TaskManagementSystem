using TaskManagement.Application.DTOs;
using TaskManagement.Application.Wrappers;
namespace TaskManagement.Application.Interfaces;
public interface ICategoryService
{
    Task<ApiResponse<IEnumerable<CategoryDto>>> GetAllCategoriesAsync(Guid userId, CancellationToken ct = default);
    Task<ApiResponse<CategoryDto>> GetCategoryByIdAsync(Guid userId, Guid id, CancellationToken ct = default);
    Task<ApiResponse<CategoryDto>> CreateCategoryAsync(Guid userId, CreateCategoryDto dto, CancellationToken ct = default);
    Task<ApiResponse<CategoryDto>> UpdateCategoryAsync(Guid userId, Guid id, UpdateCategoryDto dto, CancellationToken ct = default);
    Task<ApiResponse<bool>> DeleteCategoryAsync(Guid userId, Guid id, CancellationToken ct = default);
}
