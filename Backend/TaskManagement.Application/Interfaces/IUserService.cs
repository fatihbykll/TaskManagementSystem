using TaskManagement.Application.DTOs;
using TaskManagement.Application.Wrappers;
namespace TaskManagement.Application.Interfaces;
public interface IUserService
{
    Task<ApiResponse<UserDto>> RegisterAsync(CreateUserDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<TokenDto>> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<TokenDto>> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> LogoutAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> ForgotPasswordAsync(string email, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> ResetPasswordAsync(ResetPasswordDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<UserDto>> GetProfileAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<ApiResponse<UserDto>> UpdateProfileAsync(Guid userId, UpdateUserDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> DeactivateAsync(Guid userId, CancellationToken cancellationToken = default);
}
