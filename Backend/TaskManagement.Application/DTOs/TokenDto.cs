namespace TaskManagement.Application.DTOs;
/// <summary>
/// Başarılı kimlik doğrulama sonrası istemciye dönen token zarfı.
/// RefreshToken ile access token süresi dolduğunda yenileme yapılabilir.
/// </summary>
public class TokenDto
{
    public string AccessToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime RefreshTokenExpiresAt { get; set; }
}
