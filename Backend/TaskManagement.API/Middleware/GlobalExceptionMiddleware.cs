using System.Net;
using System.Text.Json;
using Serilog;
using TaskManagement.Application.Wrappers;
namespace TaskManagement.API.Middleware;
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    public GlobalExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Beklenmeyen bir hata oluştu. Path: {Path}", context.Request.Path);
            await HandleExceptionAsync(context, ex);
        }
    }
    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        var response = ApiResponse<object>.FailResult("Sunucu tarafında beklenmeyen bir hata oluştu. Lütfen daha sonra tekrar deneyin.");
        
        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
