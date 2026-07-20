using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Interfaces;
using TaskManagement.Application.Settings;

namespace TaskManagement.Infrastructure.Services;

/// <summary>
/// HS256 imzalı JWT token üretimi ve doğrulaması.
/// Anahtar ve konfigürasyon IOptions ile inject edilir; hardcode değer içermez.
/// </summary>
public class JwtService : IJwtService
{
    private readonly JwtSettings _settings;
    private readonly SymmetricSecurityKey _signingKey;

    public JwtService(IOptions<JwtSettings> settings)
    {
        _settings = settings.Value;

        // Startup'ta fail-fast: HS256, minimum 256-bit (32 byte) anahtar gerektirir.
        if (string.IsNullOrWhiteSpace(_settings.SecretKey) || _settings.SecretKey.Length < 32)
            throw new InvalidOperationException(
                "JwtSettings:SecretKey en az 32 karakter olmalıdır. " +
                "Gerçek değeri 'dotnet user-secrets set JwtSettings:SecretKey <value>' ile set edin.");

        _signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
    }

    public TokenDto GenerateToken(UserDto user)
    {
        var expiry = DateTime.UtcNow.AddMinutes(_settings.AccessTokenExpirationMinutes);

        var claims = new[]
        {
            // sub: RFC 7519 standardı; kullanıcının canonical identifier'ı.
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
            // jti: Token benzersizliği; ileride token revocation / blacklist için gereklidir.
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64),
        };

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            NotBefore = DateTime.UtcNow,
            Expires = expiry,
            Issuer = _settings.Issuer,
            Audience = _settings.Audience,
            SigningCredentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256)
        };

        var handler = new JwtSecurityTokenHandler();
        var token = handler.CreateToken(descriptor);

        return new TokenDto
        {
            AccessToken = handler.WriteToken(token),
            ExpiresAt = expiry
        };
    }

    public ClaimsPrincipal? GetPrincipalFromToken(string token)
    {
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = _signingKey,
            ValidateIssuer = true,
            ValidIssuer = _settings.Issuer,
            ValidateAudience = true,
            ValidAudience = _settings.Audience,
            // Refresh akışında süresi dolmuş token'dan claim okumak gerekebilir.
            ValidateLifetime = false,
            ClockSkew = TimeSpan.Zero
        };

        try
        {
            return new JwtSecurityTokenHandler()
                .ValidateToken(token, validationParameters, out _);
        }
        catch
        {
            // Manipüle edilmiş veya geçersiz token; null ile caller'a bırakılır.
            return null;
        }
    }
}
