namespace TaskManagement.Application.Settings;
/// <summary>
/// Uygulama geneli yapılandırma. IConfiguration bağımlılığı Application katmanına sızmaz;
/// Program.cs'te IOptions ile bind edilir.
/// </summary>
public class AppSettings
{
    /// <summary>Bu e-posta ile kayıt olan kullanıcı Admin rolü alır.</summary>
    public string AdminEmail { get; set; } = string.Empty;
}
