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

    /// <summary>Email ve şifre ile kimlik doğrulama. Geçerli token döner.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<TokenDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<TokenDto>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<TokenDto>.FailResult(
                ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

        var result = await _userService.LoginAsync(dto, ct);
        if (!result.Success)
            // 401: kimlik doğrulama başarısız; hata mesajı timing-safe tutulur.
            return Unauthorized(result);

        var token = _jwtService.GenerateToken(result.Data!);
        return Ok(ApiResponse<TokenDto>.SuccessResult(token, "Giriş başarılı."));
    }

    /// <summary>Authenticated kullanıcının profil bilgilerini getirir.</summary>
    [HttpGet("profile")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetProfile(CancellationToken ct)
    {
        var result = await _userService.GetProfileAsync(GetCurrentUserId(), ct);
        if (!result.Success)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>Profil güncelleme. Sadece FirstName, LastName ve Email değiştirilebilir.</summary>
    [HttpPut("profile")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<UserDto>.FailResult(
                ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

        var result = await _userService.UpdateProfileAsync(GetCurrentUserId(), dto, ct);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}
