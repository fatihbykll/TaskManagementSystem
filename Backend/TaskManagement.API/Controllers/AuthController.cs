using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Interfaces;
using TaskManagement.Application.Wrappers;
namespace TaskManagement.API.Controllers;
/// <summary>
/// Kimlik doğrulama ve kullanıcı profil yönetimi endpoint'leri.
/// </summary>
[Authorize]
public class AuthController : BaseApiController
{
    private readonly IUserService _userService;
    private readonly IJwtService _jwtService;
    public AuthController(IUserService userService, IJwtService jwtService)
    {
        _userService = userService;
        _jwtService = jwtService;
    }
    /// <summary>Yeni kullanıcı kaydı. Başarılı kayıt sonrası doğrudan token döner.</summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    [ProducesResponseType(typeof(ApiResponse<TokenDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<TokenDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] CreateUserDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<TokenDto>.FailResult(
                ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));
        var result = await _userService.RegisterAsync(dto, ct);
        if (!result.Success)
            return BadRequest(result);
        // Kayıt başarılı → anında token üret; istemci ek login isteği atmaz.
        var token = _jwtService.GenerateToken(result.Data!);
        return StatusCode(StatusCodes.Status201Created,
            ApiResponse<TokenDto>.SuccessResult(token, "Kayıt başarılı."));
    }
    /// <summary>
    /// Email ve şifre ile kimlik doğrulama.
    /// Artık hem AccessToken hem de RefreshToken döner.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    [ProducesResponseType(typeof(ApiResponse<TokenDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<TokenDto>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<TokenDto>.FailResult(
                ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));
        // LoginAsync artık token üretimini de yapıyor; controller sadece sonucu döner.
        var result = await _userService.LoginAsync(dto, ct);
        if (!result.Success)
            return Unauthorized(result);
        return Ok(result);
    }
    /// <summary>
    /// Süresi dolmuş access token'ı refresh token ile yeniler.
    /// Eski refresh token iptal edilir; yeni token çifti döner (Rotation).
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<TokenDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<TokenDto>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenDto dto, CancellationToken ct)
    {
        var result = await _userService.RefreshTokenAsync(dto.RefreshToken, ct);
        if (!result.Success)
            return Unauthorized(result);
        return Ok(result);
    }
    /// <summary>Refresh token'ı iptal ederek oturumu sonlandırır.</summary>
    [HttpPost("logout")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenDto dto, CancellationToken ct)
    {
        var result = await _userService.LogoutAsync(dto.RefreshToken, ct);
        return Ok(result);
    }
    /// <summary>Şifre sıfırlama e-postası gönderir. Kullanıcı varlığı expose edilmez.</summary>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto, CancellationToken ct)
    {
        var result = await _userService.ForgotPasswordAsync(dto.Email, ct);
        return Ok(result);
    }
    /// <summary>Token ile yeni şifre belirler.</summary>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto, CancellationToken ct)
    {
        var result = await _userService.ResetPasswordAsync(dto, ct);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }
    /// <summary>Authenticated kullanıcının profil bilgilerini getirir.</summary>
    [HttpGet("profile")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProfile(CancellationToken ct)
    {
        var result = await _userService.GetProfileAsync(GetCurrentUserId(), ct);
        if (!result.Success) return NotFound(result);
        return Ok(result);
    }
    /// <summary>Profil güncelleme.</summary>
    [HttpPut("profile")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<UserDto>.FailResult(
                ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));
        var result = await _userService.UpdateProfileAsync(GetCurrentUserId(), dto, ct);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }
}
