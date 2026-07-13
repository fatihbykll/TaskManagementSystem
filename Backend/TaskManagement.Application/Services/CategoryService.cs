using AutoMapper;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Interfaces;
using TaskManagement.Application.Wrappers;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Interfaces;

namespace TaskManagement.Application.Services;

/// <summary>
/// Kategori CRUD iş mantığı; kullanıcı izolasyonu ve scope-level isim tekiliği sağlar.
/// </summary>
public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CategoryService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<IEnumerable<CategoryDto>>> GetCategoriesByUserIdAsync(
        Guid userId, CancellationToken cancellationToken = default)
    {
        var categories = await _unitOfWork.Repository<Category>()
            .FindAsync(c => c.UserId == userId, cancellationToken);

        return ApiResponse<IEnumerable<CategoryDto>>.SuccessResult(
            _mapper.Map<IEnumerable<CategoryDto>>(categories));
    }

    public async Task<ApiResponse<CategoryDto>> GetByIdAsync(
        Guid categoryId, Guid userId, CancellationToken cancellationToken = default)
    {
        // UserId koşulu; başka kullanıcının kategorisine erişimi önler (IDOR koruması).
        var category = await _unitOfWork.Repository<Category>()
            .FirstOrDefaultAsync(c => c.Id == categoryId && c.UserId == userId, cancellationToken);

        if (category == null)
            return ApiResponse<CategoryDto>.FailResult("Kategori bulunamadı.");

        return ApiResponse<CategoryDto>.SuccessResult(_mapper.Map<CategoryDto>(category));
    }

    public async Task<ApiResponse<CategoryDto>> CreateCategoryAsync(
        Guid userId, CreateCategoryDto dto, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<Category>();

        // Orphan category oluşumunu önler; FK kısıtlaması iş katmanında erken yakalanır.
        var userExists = await _unitOfWork.Repository<User>()
            .AnyAsync(u => u.Id == userId && u.IsActive, cancellationToken);

        if (!userExists)
            return ApiResponse<CategoryDto>.FailResult("Geçerli bir kullanıcı bulunamadı.");

        // Case-insensitive duplikasyon kontrolü; aynı user scope'unda tekil isim zorunluluğu.
        var nameExists = await repo.AnyAsync(
            c => c.UserId == userId && c.Name.ToLower() == dto.Name.ToLower(),
            cancellationToken);

        if (nameExists)
            return ApiResponse<CategoryDto>.FailResult("Bu isimde bir kategori zaten mevcut.");

        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Description = dto.Description ?? string.Empty,
            Color = dto.Color ?? "#007bff",
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        await repo.AddAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<CategoryDto>.SuccessResult(
            _mapper.Map<CategoryDto>(category), "Kategori başarıyla oluşturuldu.");
    }

    public async Task<ApiResponse<CategoryDto>> UpdateCategoryAsync(
        Guid categoryId, Guid userId, UpdateCategoryDto dto, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<Category>();

        // Ownership doğrulaması; IDOR'a karşı yeterlik ve kayıt varlığını tek sorguda kontrol eder.
        var category = await repo.FirstOrDefaultAsync(
            c => c.Id == categoryId && c.UserId == userId, cancellationToken);

        if (category == null)
            return ApiResponse<CategoryDto>.FailResult(
                "Kategori bulunamadı veya bu kategoriye erişim yetkiniz yok.");

        // Değişiklik yoksa gereksiz duplikasyon sorgusu çalıştırılmaz.
        if (!string.IsNullOrWhiteSpace(dto.Name) && dto.Name.ToLower() != category.Name.ToLower())
        {
            var nameExists = await repo.AnyAsync(
                c => c.UserId == userId && c.Name.ToLower() == dto.Name.ToLower() && c.Id != categoryId,
                cancellationToken);

            if (nameExists)
                return ApiResponse<CategoryDto>.FailResult("Bu isimde bir kategori zaten mevcut.");

            category.Name = dto.Name;
        }

        if (!string.IsNullOrWhiteSpace(dto.Description)) category.Description = dto.Description;
        if (!string.IsNullOrWhiteSpace(dto.Color)) category.Color = dto.Color;

        repo.Update(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<CategoryDto>.SuccessResult(
            _mapper.Map<CategoryDto>(category), "Kategori başarıyla güncellendi.");
    }

    public async Task<ApiResponse<bool>> DeleteCategoryAsync(
        Guid categoryId, Guid userId, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<Category>();

        // Ownership doğrulaması; IDOR'a karşı yeterlik ve kayıt varlığını tek sorguda kontrol eder.
        var category = await repo.FirstOrDefaultAsync(
            c => c.Id == categoryId && c.UserId == userId, cancellationToken);

        if (category == null)
            return ApiResponse<bool>.FailResult(
                "Kategori bulunamadı veya bu kategoriye erişim yetkiniz yok.");

        repo.Delete(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // EF Core OnDelete(SetNull) konfigürasyonu; silme cascade'i task'lara yayılmaz, CategoryId NULL'a çekilir.
        return ApiResponse<bool>.SuccessResult(true, "Kategori başarıyla silindi.");
    }
}
