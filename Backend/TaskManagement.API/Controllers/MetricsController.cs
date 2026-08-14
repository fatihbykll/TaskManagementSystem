using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Wrappers;
using TaskManagement.Infrastructure.Data;
namespace TaskManagement.API.Controllers;
/// <summary>
/// Sistem sağlık ve metrik bilgilerini döner.
/// Prometheus, Grafana veya herhangi bir monitoring sistemi bu endpoint'i okuyabilir.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class MetricsController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private static readonly DateTime _startTime = DateTime.UtcNow;
    public MetricsController(ApplicationDbContext db) => _db = db;
    /// <summary>Uygulama çalışma süresi ve temel sistem metrikleri.</summary>
    [HttpGet]
    public IActionResult GetMetrics()
    {
        var uptime = DateTime.UtcNow - _startTime;
        return Ok(new
        {
            status = "healthy",
            uptimeSeconds = (long)uptime.TotalSeconds,
            uptimeFormatted = $"{uptime.Days}g {uptime.Hours}s {uptime.Minutes}dk",
            serverTime = DateTime.UtcNow,
            environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
            dotnetVersion = Environment.Version.ToString()
        });
    }
    /// <summary>DB row sayıları — basit veri büyüklüğü metriği.</summary>
    [HttpGet("db")]
    public async Task<IActionResult> GetDbMetrics(CancellationToken ct)
    {
        var metrics = new
        {
            users = await _db.Users.CountAsync(ct),
            tasks = await _db.Tasks.CountAsync(ct),
            comments = await _db.TaskComments.CountAsync(ct),
            attachments = await _db.TaskAttachments.CountAsync(ct),
            activeRefreshTokens = await _db.RefreshTokens
                .CountAsync(r => !r.IsRevoked && r.ExpiresAt > DateTime.UtcNow, ct)
        };
        return Ok(ApiResponse<object>.SuccessResult(metrics, "DB metrikleri başarıyla alındı."));
    }
}
