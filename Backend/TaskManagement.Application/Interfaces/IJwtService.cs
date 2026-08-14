using System.Security.Claims;
using TaskManagement.Application.DTOs;
namespace TaskManagement.Application.Interfaces;
public interface IJwtService
{
    /// <summary>Kullanıcı kimlik bilgilerinden imzalı JWT access token üretir.</summary>
    TokenDto GenerateToken(UserDto user);
    /// <summary>Kriptografik olarak güvenli bir refresh token string'i üretir.</summary>
    string GenerateRefreshToken();
    /// <summary>
    /// Token string'inden ClaimsPrincipal çıkarır.
    /// Refresh senaryolarında süresi dolmuş token'lar da işlenir; lifetime kontrolü devre dışıdır.
    /// </summary>
    ClaimsPrincipal? GetPrincipalFromToken(string token);
}
