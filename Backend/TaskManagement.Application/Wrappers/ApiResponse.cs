namespace TaskManagement.Application.Wrappers;

/// <summary>
/// Tüm API yanıtlarını saran standart zarf. İstemci tarafında tutarlı hata/başarı
/// ayrımı sağlamak ve response şemasını sabitlemek için zorunludur.
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; private set; }
    public string? Message { get; private set; }
    public T? Data { get; private set; }
    public List<string> Errors { get; private set; } = new();

    private ApiResponse() { }

    /// <summary>Veri dönen başarılı yanıt factory metodu.</summary>
    public static ApiResponse<T> SuccessResult(T data, string message = "İşlem başarılı.")
        => new() { Success = true, Data = data, Message = message };

    /// <summary>Veri içermeyen başarılı yanıt (silme, durum güncelleme vb.).</summary>
    public static ApiResponse<T> SuccessResult(string message = "İşlem başarılı.")
        => new() { Success = true, Message = message };

    /// <summary>Tek hatalı başarısız yanıt.</summary>
    public static ApiResponse<T> FailResult(string error)
        => new() { Success = false, Errors = new List<string> { error } };

    /// <summary>Birden fazla hata içeren başarısız yanıt; validasyon hatalarını toplu taşımak için kullanılır.</summary>
    public static ApiResponse<T> FailResult(List<string> errors)
        => new() { Success = false, Errors = errors };
}
