using System.Text.Json;
using TaskManagement.Application.Wrappers;

namespace TaskManagement.API.Middleware;

/// <summary>
/// Pipeline'daki tüm işlenmemiş exception'ları merkezi olarak yakalar.
/// İstemci her zaman ApiResponse formatında yanıt alır; exception detayı
/// production ortamında expose edilmez.
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
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            // Structured logging; korelasyon için request path de kaydedilir.
            _logger.LogError(ex, "İşlenmemiş exception — Path: {Path}", context.Request.Path);
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
