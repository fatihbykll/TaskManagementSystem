using System.Diagnostics;
using System.Text.Json;
using TaskManagement.Application.Wrappers;

namespace TaskManagement.API.Middleware;

/// <summary>
/// Pipeline'daki tüm işlenmemiş exception'ları merkezi olarak yakalar.
/// Her isteğe correlation ID atar; hata takibi ve distributed tracing için zorunludur.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _env;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // X-Correlation-Id header yoksa yeni GUID üretilir; istemci hata raporlamasında kullanabilir.
        var correlationId = context.Request.Headers["X-Correlation-Id"].FirstOrDefault()
                            ?? Guid.NewGuid().ToString();

        context.Response.Headers["X-Correlation-Id"] = correlationId;

        var sw = Stopwatch.StartNew();

        try
        {
            await _next(context);
            sw.Stop();

            // Başarılı isteklerin süresi bilgi amaçlı loglanır; yavaş endpoint tespiti için kullanılır.
            _logger.LogInformation(
                "HTTP {Method} {Path} → {StatusCode} | {Elapsed}ms | CorrelationId: {CorrelationId}",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                sw.ElapsedMilliseconds,
                correlationId);
        }
        catch (Exception ex)
        {
            sw.Stop();

            // Structured logging: correlation ID ile exception izlenebilir.
            _logger.LogError(ex,
                "İşlenmemiş exception — {Method} {Path} | {Elapsed}ms | CorrelationId: {CorrelationId}",
                context.Request.Method,
                context.Request.Path,
                sw.ElapsedMilliseconds,
                correlationId);

            await WriteErrorResponseAsync(context, ex);
        }
    }

    private async Task WriteErrorResponseAsync(HttpContext context, Exception ex)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        // Stack trace ve internal mesaj production'da gizlenir; bilgi sızıntısını önler.
        var message = _env.IsDevelopment()
            ? $"Sunucu hatası: {ex.Message}"
            : "Beklenmedik bir hata oluştu. Lütfen daha sonra tekrar deneyin.";

        var body = JsonSerializer.Serialize(
            ApiResponse<object>.FailResult(message),
            _jsonOptions);

        await context.Response.WriteAsync(body);
    }
}
