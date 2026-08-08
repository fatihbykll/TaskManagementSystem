using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Interfaces;
namespace TaskManagement.API.Controllers;
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CategoriesController : BaseApiController
{
    private readonly ICategoryService _categoryService;
    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _categoryService.GetAllCategoriesAsync(GetCurrentUserId(), ct);
        return result.Success ? Ok(result) : BadRequest(result);
    }
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _categoryService.GetCategoryByIdAsync(GetCurrentUserId(), id, ct);
        return result.Success ? Ok(result) : NotFound(result);
    }
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCategoryDto dto, CancellationToken ct)
    {
        var result = await _categoryService.CreateCategoryAsync(GetCurrentUserId(), dto, ct);
        return result.Success
            ? StatusCode(StatusCodes.Status201Created, result)
            : BadRequest(result);
    }
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCategoryDto dto, CancellationToken ct)
    {
        var result = await _categoryService.UpdateCategoryAsync(GetCurrentUserId(), id, dto, ct);
        return result.Success ? Ok(result) : NotFound(result);
    }
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await _categoryService.DeleteCategoryAsync(GetCurrentUserId(), id, ct);
        return result.Success ? Ok(result) : NotFound(result);
    }
}
