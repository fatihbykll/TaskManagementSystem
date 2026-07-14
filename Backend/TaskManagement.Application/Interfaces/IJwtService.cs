using System.Security.Claims;
using TaskManagement.Application.DTOs;

namespace TaskManagement.Application.Interfaces;

/// <summary>
/// JWT token üretimi ve doğrulaması için servis sözleşmesi.
/// Application katmanı token imzalama detaylarından izole kalır;
/// implementasyon Infrastructure'da değiştirilebilir.
/// </summary>
public interface IJwtService
{
    /// <summary>
    /// Kullanıcı kimlik bilgilerinden imzalı JWT access token üretir.
    /// </summary>
    TokenDto GenerateToken(UserDto user);

    /// <summary>
    /// Token string'inden ClaimsPrincipal çıkarır.
    /// Refresh senaryolarında süresi dolmuş token'lar da işlenir; lifetime kontrolü devre dışıdır.
    /// </summary>
    ClaimsPrincipal? GetPrincipalFromToken(string token);
}
