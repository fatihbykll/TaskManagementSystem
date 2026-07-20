using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Interfaces;
using TaskManagement.Application.Wrappers;

namespace TaskManagement.API.Controllers;

/// <summary>
/// Görev yorumları. Tüm işlemler JWT ile authenticated kullanıcıya scope'lanır.
/// </summary>
[Authorize]
[Route("api/tasks/{taskId:guid}/comments")]
[ApiController]
public class TaskCommentsController : BaseApiController
{
    private readonly ICommentService _commentService;

    public TaskCommentsController(ICommentService commentService)
    {
        _commentService = commentService;
    }

    /// <summary>Göreve ait yorumları listeler. Yalnızca görev sahibi görebilir.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CommentDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetComments(Guid taskId, CancellationToken ct)
    {
        var result = await _commentService.GetCommentsByTaskIdAsync(taskId, GetCurrentUserId(), ct);
        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>Göreve yorum ekler. UserId JWT'den alınır.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CommentDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<CommentDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddComment(
        Guid taskId, [FromBody] CreateCommentDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<CommentDto>.FailResult(
                ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

        var result = await _commentService.AddCommentAsync(taskId, GetCurrentUserId(), dto, ct);
        if (!result.Success)
            return BadRequest(result);

        return StatusCode(StatusCodes.Status201Created, result);
    }

    /// <summary>Yorumu siler. Yalnızca yorumun sahibi silebilir.</summary>
    [HttpDelete("{commentId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteComment(Guid taskId, Guid commentId, CancellationToken ct)
    {
        var result = await _commentService.DeleteCommentAsync(commentId, GetCurrentUserId(), ct);
        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }
}
