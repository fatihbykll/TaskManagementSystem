namespace TaskManagement.Application.Settings;

/// <summary>
/// appsettings.json → JwtSettings bölümünün strongly-typed karşılığı.
/// IOptions&lt;JwtSettings&gt; üzerinden inject edilir; magic string kullanımını önler.
/// </summary>
public class JwtSettings
{
    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int AccessTokenExpirationMinutes { get; set; } = 30;
}
