using TaskManagement.Application.DTOs;
using TaskManagement.Application.Wrappers;

namespace TaskManagement.Application.Interfaces;

/// <summary>
/// Kategori yönetimi için servis sözleşmesi.
/// </summary>
public interface ICategoryService
{
    /// <summary>Kullanıcıya ait tüm kategorileri getirir.</summary>
    Task<ApiResponse<IEnumerable<CategoryDto>>> GetCategoriesByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Id'ye göre tek bir kategori getirir.</summary>
    Task<ApiResponse<CategoryDto>> GetByIdAsync(Guid categoryId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Yeni kategori oluşturur. Aynı kullanıcıda isim benzersizliği kontrol edilir.</summary>
    Task<ApiResponse<CategoryDto>> CreateCategoryAsync(Guid userId, CreateCategoryDto dto, CancellationToken cancellationToken = default);

    /// <summary>Kategori bilgilerini günceller.</summary>
    Task<ApiResponse<CategoryDto>> UpdateCategoryAsync(Guid categoryId, Guid userId, UpdateCategoryDto dto, CancellationToken cancellationToken = default);

    /// <summary>Kategoriyi siler. Kategoriye bağlı görevlerin CategoryId'si NULL olur.</summary>
    Task<ApiResponse<bool>> DeleteCategoryAsync(Guid categoryId, Guid userId, CancellationToken cancellationToken = default);
}
