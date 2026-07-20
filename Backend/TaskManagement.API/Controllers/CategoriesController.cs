using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Interfaces;
using TaskManagement.Application.Wrappers;

namespace TaskManagement.API.Controllers;

/// <summary>
/// Kategori CRUD endpoint'leri. Tüm işlemler JWT ile authenticated kullanıcıya scope'lanır.
/// </summary>
[Authorize]
public class CategoriesController : BaseApiController
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    /// <summary>Kullanıcıya ait tüm kategorileri listeler.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CategoryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _categoryService.GetCategoriesByUserIdAsync(GetCurrentUserId(), ct);
        return Ok(result);
    }

    /// <summary>Id'ye göre tek kategori getirir. Sahiplik kontrolü servis katmanında yapılır.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<CategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CategoryDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _categoryService.GetByIdAsync(id, GetCurrentUserId(), ct);
        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>Yeni kategori oluşturur. UserId JWT'den alınır; request body'de gönderilmez.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CategoryDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<CategoryDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateCategoryDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<CategoryDto>.FailResult(
                ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

        var result = await _categoryService.CreateCategoryAsync(GetCurrentUserId(), dto, ct);
        if (!result.Success)
            return BadRequest(result);

        return StatusCode(StatusCodes.Status201Created, result);
    }

    /// <summary>Kategori günceller. Sadece sahibi güncelleyebilir.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<CategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CategoryDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<CategoryDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCategoryDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<CategoryDto>.FailResult(
                ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

        var result = await _categoryService.UpdateCategoryAsync(id, GetCurrentUserId(), dto, ct);
        if (!result.Success)
            // Servis; "bulunamadı" ve "iş kuralı ihlali" mesajlarını birlikte döner.
            return result.Errors.Any(e => e.Contains("bulunamadı") || e.Contains("yetki"))
                ? NotFound(result)
                : BadRequest(result);

        return Ok(result);
    }

    /// <summary>Kategoriyi siler. Bağlı görevlerin CategoryId'si NULL'a çekilir (OnDelete SetNull).</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await _categoryService.DeleteCategoryAsync(id, GetCurrentUserId(), ct);
        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }
}
