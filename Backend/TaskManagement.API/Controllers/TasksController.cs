using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Application.Common;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Interfaces;
using TaskManagement.Application.Wrappers;
namespace TaskManagement.API.Controllers;
/// <summary>
/// Görev CRUD, sayfalı filtreleme, durum yönetimi, istatistik ve overdue endpoint'leri.
/// </summary>
[Authorize]
public class TasksController : BaseApiController
{
    private readonly ITaskService _taskService;
    private readonly IReportService _reportService;
    public TasksController(ITaskService taskService, IReportService reportService)
    {
        _taskService = taskService;
        _reportService = reportService;
    }
    /// <summary>Filtrelenmiş ve sayfalı görev listesi.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<TaskItemDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] TaskFilterDto filter, CancellationToken ct)
    {
        var result = await _taskService.GetTasksByUserIdAsync(GetCurrentUserId(), filter, ct);
        return Ok(result);
    }
    /// <summary>Kullanıcıya ait görev istatistiklerini döner.</summary>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(ApiResponse<TaskStatisticsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatistics(CancellationToken ct)
    {
        var result = await _taskService.GetStatisticsAsync(GetCurrentUserId(), ct);
        return Ok(result);
    }
    /// <summary>Kullanıcının verimlilik skorunu ve streak bilgisini döner.</summary>
    [HttpGet("productivity")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProductivity(CancellationToken ct)
    {
        var result = await _reportService.GetUserProductivityAsync(GetCurrentUserId(), ct);
        return Ok(ApiResponse<object>.SuccessResult(result, "Verimlilik skoru hesaplandı."));
    }
    /// <summary>Vadesi geçmiş (overdue) görevleri listeler.</summary>
    [HttpGet("overdue")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<TaskItemDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOverdue(CancellationToken ct)
    {
        var result = await _taskService.GetOverdueTasksAsync(GetCurrentUserId(), ct);
        return Ok(result);
    }
    /// <summary>Id'ye göre tek görev getirir.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<TaskItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<TaskItemDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _taskService.GetTaskByIdAsync(id, GetCurrentUserId(), ct);
        if (!result.Success) return NotFound(result);
        return Ok(result);
    }
    /// <summary>Yeni görev oluşturur.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<TaskItemDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<TaskItemDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateTaskDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<TaskItemDto>.FailResult(
                ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));
        var result = await _taskService.CreateTaskAsync(GetCurrentUserId(), dto, ct);
        if (!result.Success) return BadRequest(result);
        return StatusCode(StatusCodes.Status201Created, result);
    }
    /// <summary>Görevi günceller.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<TaskItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<TaskItemDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTaskDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<TaskItemDto>.FailResult(
                ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));
        var result = await _taskService.UpdateTaskAsync(id, GetCurrentUserId(), dto, ct);
        if (!result.Success)
            return result.Errors.Any(e => e.Contains("bulunamadı") || e.Contains("yetki"))
                ? NotFound(result) : BadRequest(result);
        return Ok(result);
    }
    /// <summary>Görevi siler.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await _taskService.DeleteTaskAsync(id, GetCurrentUserId(), ct);
        if (!result.Success) return NotFound(result);
        return Ok(result);
    }
    /// <summary>Görev durumunu günceller.</summary>
    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(typeof(ApiResponse<TaskItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<TaskItemDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateTaskStatusDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<TaskItemDto>.FailResult(
                ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));
        var result = await _taskService.UpdateTaskStatusAsync(id, GetCurrentUserId(), dto.Status, ct);
        if (!result.Success)
            return result.Errors.Any(e => e.Contains("bulunamadı"))
                ? NotFound(result) : BadRequest(result);
        return Ok(result);
    }
}
