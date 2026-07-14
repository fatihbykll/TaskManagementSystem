namespace TaskManagement.Application.DTOs;

/// <summary>
/// Başarılı kimlik doğrulama sonrası istemciye dönen token zarfı.
/// ExpiresAt istemcinin proaktif token yenileme zamanlaması için gereklidir.
/// </summary>
public class TokenDto
{
    public string AccessToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}
