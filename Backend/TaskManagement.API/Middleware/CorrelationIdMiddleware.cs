using Serilog.Context;
namespace TaskManagement.API.Middleware;
/// <summary>
/// Her HTTP isteğine benzersiz bir Correlation ID atar.
/// X-Correlation-ID header'ı varsa onu kullanır, yoksa yeni UUID üretir.
/// Tüm log satırlarına ve response header'larına eklenir.
/// </summary>
public class CorrelationIdMiddleware
{
    private const string CorrelationIdHeader = "X-Correlation-ID";
    private readonly RequestDelegate _next;
    public CorrelationIdMiddleware(RequestDelegate next) => _next = next;
    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers[CorrelationIdHeader].FirstOrDefault()
                            ?? Guid.NewGuid().ToString("N")[..12];
        // Serilog LogContext'e ekle — template'deki {CorrelationId} bu değeri alır
        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            context.Response.Headers[CorrelationIdHeader] = correlationId;
            context.Items["CorrelationId"] = correlationId;
            await _next(context);
        }
    }
}
