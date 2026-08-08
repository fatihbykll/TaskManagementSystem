using AutoMapper;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Interfaces;
using TaskManagement.Application.Wrappers;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Interfaces;
namespace TaskManagement.Application.Services;
public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    public CategoryService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }
    public async Task<ApiResponse<IEnumerable<CategoryDto>>> GetAllCategoriesAsync(Guid userId, CancellationToken ct = default)
    {
        var repo = _unitOfWork.Repository<Category>();
        var all  = await repo.GetAllAsync(ct);
        var list = all.Where(c => c.UserId == userId)
                      .OrderByDescending(c => c.CreatedAt)
                      .ToList();
        return ApiResponse<IEnumerable<CategoryDto>>.SuccessResult(_mapper.Map<IEnumerable<CategoryDto>>(list));
    }
    public async Task<ApiResponse<CategoryDto>> GetCategoryByIdAsync(Guid userId, Guid id, CancellationToken ct = default)
    {
        var repo     = _unitOfWork.Repository<Category>();
        var all      = await repo.GetAllAsync(ct);
        var category = all.FirstOrDefault(c => c.Id == id && c.UserId == userId);
        if (category == null)
            return ApiResponse<CategoryDto>.FailResult("Kategori bulunamadı.");
        return ApiResponse<CategoryDto>.SuccessResult(_mapper.Map<CategoryDto>(category));
    }
    public async Task<ApiResponse<CategoryDto>> CreateCategoryAsync(Guid userId, CreateCategoryDto dto, CancellationToken ct = default)
    {
        var category = new Category
        {
            Name        = dto.Name,
            Description = dto.Description,
            Color       = dto.Color,
            UserId      = userId
        };
        await _unitOfWork.Repository<Category>().AddAsync(category, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return ApiResponse<CategoryDto>.SuccessResult(_mapper.Map<CategoryDto>(category), "Kategori oluşturuldu.");
    }
    public async Task<ApiResponse<CategoryDto>> UpdateCategoryAsync(Guid userId, Guid id, UpdateCategoryDto dto, CancellationToken ct = default)
    {
        var repo     = _unitOfWork.Repository<Category>();
        var all      = await repo.GetAllAsync(ct);
        var category = all.FirstOrDefault(c => c.Id == id && c.UserId == userId);
        if (category == null)
            return ApiResponse<CategoryDto>.FailResult("Kategori bulunamadı.");
        category.Name        = dto.Name;
        category.Description = dto.Description;
        category.Color       = dto.Color;
        repo.Update(category);
        await _unitOfWork.SaveChangesAsync(ct);
        return ApiResponse<CategoryDto>.SuccessResult(_mapper.Map<CategoryDto>(category), "Kategori güncellendi.");
    }
    public async Task<ApiResponse<bool>> DeleteCategoryAsync(Guid userId, Guid id, CancellationToken ct = default)
    {
        var repo     = _unitOfWork.Repository<Category>();
        var all      = await repo.GetAllAsync(ct);
        var category = all.FirstOrDefault(c => c.Id == id && c.UserId == userId);
        if (category == null)
            return ApiResponse<bool>.FailResult("Kategori bulunamadı.");
        repo.Delete(category);
        await _unitOfWork.SaveChangesAsync(ct);
        return ApiResponse<bool>.SuccessResult(true, "Kategori silindi.");
    }
}
