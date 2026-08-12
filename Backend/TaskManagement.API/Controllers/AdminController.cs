using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
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
    private readonly IDistributedCache _cache;

    public AdminController(ApplicationDbContext db, IDistributedCache cache)
    {
        _db = db;
        _cache = cache;
    }

    /// <summary>Tüm kayıtlı kullanıcıları listeler. (Admin only)</summary>
    [HttpGet("users")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAllUsers(CancellationToken ct)
    {
        var cacheKey = "admin:users_list";
        var cachedData = await _cache.GetStringAsync(cacheKey, ct);

        if (!string.IsNullOrEmpty(cachedData))
        {
            var usersFromCache = JsonSerializer.Deserialize<object>(cachedData);
            return Ok(ApiResponse<object>.SuccessResult(usersFromCache, "Kullanıcılar cache'den getirildi."));
        }

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

        var cacheOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60)
        };
        await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(users), cacheOptions, ct);

        return Ok(ApiResponse<object>.SuccessResult(users, $"{users.Count} kullanıcı DB'den listelendi."));
    }

    /// <summary>Sistem geneli istatistikler. (Admin only)</summary>
    [HttpGet("statistics")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetSystemStatistics(CancellationToken ct)
    {
        var cacheKey = "admin:statistics";
        var cachedData = await _cache.GetStringAsync(cacheKey, ct);

        if (!string.IsNullOrEmpty(cachedData))
        {
            var statsFromCache = JsonSerializer.Deserialize<object>(cachedData);
            return Ok(ApiResponse<object>.SuccessResult(statsFromCache, "İstatistikler cache'den getirildi."));
        }

        var stats = new
        {
            TotalUsers    = await _db.Users.CountAsync(ct),
            TotalTasks    = await _db.Tasks.CountAsync(ct),
            TotalComments = await _db.TaskComments.CountAsync(ct),
            TotalFiles    = await _db.TaskAttachments.CountAsync(ct),
        };

        var cacheOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60)
        };
        await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(stats), cacheOptions, ct);

        return Ok(ApiResponse<object>.SuccessResult(stats, "İstatistikler DB'den hesaplandı."));
    }
}
