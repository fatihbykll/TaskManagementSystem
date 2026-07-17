namespace TaskManagement.Application.Models;

/// <summary>
/// ASP.NET Core IFormFile'dan bağımsız dosya yükleme zarfı.
/// Application katmanı web framework'e bağımlı kalmaz; Controller bridge görevi üstlenir.
/// </summary>
public record FileUploadRequest(
    Stream Content,
    string FileName,
    string ContentType,
    long Size
);
