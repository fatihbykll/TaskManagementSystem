using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Interfaces;
using TaskManagement.Application.Wrappers;

namespace TaskManagement.API.Controllers;

/// <summary>
/// Görev CRUD ve durum geçiş endpoint'leri. Tüm işlemler JWT ile authenticated kullanıcıya scope'lanır.
/// </summary>
[Authorize]
public class TasksController : BaseApiController
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    /// <summary>
    /// Kullanıcının görevlerini filtreli listeler.
    /// Query string: ?status=0&amp;priority=2&amp;categoryId=...&amp;startDate=...&amp;endDate=...
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<TaskItemDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] TaskFilterDto filter, CancellationToken ct)
    {
        var result = await _taskService.GetTasksByUserIdAsync(GetCurrentUserId(), filter, ct);
        return Ok(result);
    }

    /// <summary>Id'ye göre tek görev getirir. Sahiplik kontrolü servis katmanında yapılır.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<TaskItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<TaskItemDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _taskService.GetTaskByIdAsync(id, GetCurrentUserId(), ct);
        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>Yeni görev oluşturur. UserId JWT'den alınır; Status her zaman Pending başlar.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<TaskItemDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<TaskItemDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateTaskDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<TaskItemDto>.FailResult(
                ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

        var result = await _taskService.CreateTaskAsync(GetCurrentUserId(), dto, ct);
        if (!result.Success)
            return BadRequest(result);

        return StatusCode(StatusCodes.Status201Created, result);
    }

    /// <summary>Görevi günceller. Durum değişikliği için PATCH /status kullanılmalıdır.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<TaskItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<TaskItemDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<TaskItemDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTaskDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<TaskItemDto>.FailResult(
                ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

        var result = await _taskService.UpdateTaskAsync(id, GetCurrentUserId(), dto, ct);
        if (!result.Success)
            return result.Errors.Any(e => e.Contains("bulunamadı") || e.Contains("yetki"))
                ? NotFound(result)
                : BadRequest(result);

        return Ok(result);
    }

    /// <summary>Görevi siler. Bağlı yorum ve ekler cascade silinir (DB konfigürasyonu gereği).</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await _taskService.DeleteTaskAsync(id, GetCurrentUserId(), ct);
        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// Görev durumunu günceller. PUT'tan ayrı tutulur çünkü durum geçişi
    /// CompletedAt audit alanını otomatik set eden ayrı iş mantığı içerir.
    /// </summary>
    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(typeof(ApiResponse<TaskItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<TaskItemDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<TaskItemDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(
        Guid id, [FromBody] UpdateTaskStatusDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<TaskItemDto>.FailResult(
                ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

        var result = await _taskService.UpdateTaskStatusAsync(id, GetCurrentUserId(), dto.Status, ct);
        if (!result.Success)
            return result.Errors.Any(e => e.Contains("bulunamadı"))
                ? NotFound(result)
                : BadRequest(result);

        return Ok(result);
    }
}
