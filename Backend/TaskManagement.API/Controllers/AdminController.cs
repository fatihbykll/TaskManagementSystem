using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Wrappers;
using TaskManagement.Infrastructure.Data;
namespace TaskManagement.API.Controllers;
/// <summary>
/// Yalnızca Admin rolüne açık endpoint'ler.
/// [Authorize(Roles = "Admin")]: JWT'deki "role" claim "Admin" değilse 403 döner.
/// </summary>
[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/[controller]")]
public class AdminController : BaseApiController
{
    private readonly ApplicationDbContext _db;
    public AdminController(ApplicationDbContext db)
    {
        _db = db;
    }
    /// <summary>Tüm kayıtlı kullanıcıları listeler. (Admin only)</summary>
    [HttpGet("users")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAllUsers(CancellationToken ct)
    {
        var users = await _db.Users
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new
            {
                u.Id,
                u.Username,
                u.Email,
                u.FirstName,
                u.LastName,
                Role = u.Role.ToString(),
                u.IsActive,
                u.CreatedAt
            })
            .ToListAsync(ct);
        return Ok(ApiResponse<object>.SuccessResult(users, $"{users.Count} kullanıcı listelendi."));
    }
    /// <summary>Sistem geneli istatistikler. (Admin only)</summary>
    [HttpGet("statistics")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetSystemStatistics(CancellationToken ct)
    {
        var stats = new
        {
            TotalUsers    = await _db.Users.CountAsync(ct),
            TotalTasks    = await _db.Tasks.CountAsync(ct),
            TotalComments = await _db.TaskComments.CountAsync(ct),
            TotalFiles    = await _db.TaskAttachments.CountAsync(ct),
        };
        return Ok(ApiResponse<object>.SuccessResult(stats));
    }
}
