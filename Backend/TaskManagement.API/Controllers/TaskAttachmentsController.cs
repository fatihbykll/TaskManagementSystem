using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Interfaces;
using TaskManagement.Application.Models;
using TaskManagement.Application.Wrappers;

namespace TaskManagement.API.Controllers;

/// <summary>
/// Görev dosya eklentileri. multipart/form-data ile yükleme desteklenir.
/// Controller, IFormFile'ı framework-agnostic FileUploadRequest'e dönüştürür.
/// </summary>
[Authorize]
[Route("api/tasks/{taskId:guid}/attachments")]
[ApiController]
public class TaskAttachmentsController : BaseApiController
{
    private readonly IAttachmentService _attachmentService;

    public TaskAttachmentsController(IAttachmentService attachmentService)
    {
        _attachmentService = attachmentService;
    }

    /// <summary>Göreve ait tüm dosya eklerini listeler.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<TaskAttachmentDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAttachments(Guid taskId, CancellationToken ct)
    {
        var result = await _attachmentService.GetAttachmentsByTaskIdAsync(taskId, GetCurrentUserId(), ct);
        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// Dosya yükler. Swagger'dan test için "multipart/form-data" seçilmelidir.
    /// Max boyut: 10 MB. İzin verilen tipler: pdf, jpg, jpeg, png, docx, xlsx, txt.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<TaskAttachmentDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<TaskAttachmentDto>), StatusCodes.Status400BadRequest)]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> Upload(Guid taskId, IFormFile file, CancellationToken ct)
    {
        if (file == null || file.Length == 0)
            return BadRequest(ApiResponse<TaskAttachmentDto>.FailResult("Dosya seçilmedi."));

        // IFormFile → FileUploadRequest: ASP.NET Core tipi Application katmanına sızmaz.
        var fileRequest = new FileUploadRequest(
            file.OpenReadStream(),
            file.FileName,
            file.ContentType,
            file.Length);

        var result = await _attachmentService.UploadAttachmentAsync(taskId, GetCurrentUserId(), fileRequest, ct);
        if (!result.Success)
            return BadRequest(result);

        return StatusCode(StatusCodes.Status201Created, result);
    }

    /// <summary>Dosyayı doğrudan indirir. Content-Disposition: attachment header'ı ile sunulur.</summary>
    [HttpGet("{attachmentId:guid}/download")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Download(Guid taskId, Guid attachmentId, CancellationToken ct)
    {
        var result = await _attachmentService.GetAttachmentFileAsync(attachmentId, GetCurrentUserId(), ct);
        if (!result.Success)
            return NotFound(result);

        var (filePath, contentType, fileName) = result.Data;
        // PhysicalFile: stream yönetimi Controller'da kalır; servis sadece path döndürür.
        return PhysicalFile(filePath, contentType, fileName);
    }

    /// <summary>Dosyayı ve DB kaydını siler. Görev sahibi eki silebilir.</summary>
    [HttpDelete("{attachmentId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid taskId, Guid attachmentId, CancellationToken ct)
    {
        var result = await _attachmentService.DeleteAttachmentAsync(attachmentId, GetCurrentUserId(), ct);
        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }
}
