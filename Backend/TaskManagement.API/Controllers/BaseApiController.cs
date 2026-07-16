using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace TaskManagement.API.Controllers;

/// <summary>
/// Tüm controller'ların miras aldığı taban sınıf.
/// Tekrar eden claim okuma ve route prefix mantığını merkezileştirir.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    /// <summary>
    /// JWT sub claim'inden authenticated kullanıcının Id'sini çeker.
    /// [Authorize] attribute varlığında token geçerliyse claim her zaman mevcuttur.
    /// </summary>
    protected Guid GetCurrentUserId()
    {
        var claim = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                    ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.Parse(claim!);
    }
}
