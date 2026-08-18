using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Interfaces;
using TaskManagement.Application.Wrappers;
namespace TaskManagement.API.Controllers;
/// <summary>
/// Görevleri PDF ve Excel formatında dışa aktarma endpoint'leri.
/// </summary>
[Authorize]
public class ExportController : BaseApiController
{
    private readonly ITaskService _taskService;
    private readonly IExportService _exportService;
    public ExportController(ITaskService taskService, IExportService exportService)
    {
        _taskService = taskService;
        _exportService = exportService;
    }
    /// <summary>Kullanıcının görevlerini PDF formatında indirir (max 50 kayıt).</summary>
    [HttpGet("pdf")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportPdf(CancellationToken ct)
    {
        var filter = new TaskFilterDto { PageNumber = 1, PageSize = 50 };
        var result = await _taskService.GetTasksByUserIdAsync(GetCurrentUserId(), filter, ct);
        if (!result.Success)
            return BadRequest(ApiResponse<bool>.FailResult(result.Message));
        var pdf = _exportService.ExportTasksToPdf(result.Data!.Data, "Görevlerim");
        return File(pdf, "application/pdf", $"gorevler_{DateTime.Now:yyyyMMdd}.pdf");
    }
    /// <summary>Kullanıcının görevlerini Excel formatında indirir (max 50 kayıt).</summary>
    [HttpGet("excel")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportExcel(CancellationToken ct)
    {
        var filter = new TaskFilterDto { PageNumber = 1, PageSize = 50 };
        var result = await _taskService.GetTasksByUserIdAsync(GetCurrentUserId(), filter, ct);
        if (!result.Success)
            return BadRequest(ApiResponse<bool>.FailResult(result.Message));
        var excel = _exportService.ExportTasksToExcel(result.Data!.Data);
        return File(excel,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"gorevler_{DateTime.Now:yyyyMMdd}.xlsx");
    }
}
