using TaskManagement.Application.DTOs;
using TaskManagement.Application.Wrappers;

namespace TaskManagement.Application.Interfaces;

/// <summary>
/// Kullanıcı yönetimi için servis sözleşmesi.
/// </summary>
public interface IUserService
{
    /// <summary>Yeni kullanıcı kaydeder. Email ve Username benzersizlik kontrolü yapar.</summary>
    Task<ApiResponse<UserDto>> RegisterAsync(CreateUserDto dto, CancellationToken cancellationToken = default);

    /// <summary>Email ve şifre ile kimlik doğrulaması yapar.</summary>
    Task<ApiResponse<UserDto>> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default);

    /// <summary>Id'ye göre kullanıcı profilini getirir.</summary>
    Task<ApiResponse<UserDto>> GetProfileAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Kullanıcı bilgilerini günceller.</summary>
    Task<ApiResponse<UserDto>> UpdateProfileAsync(Guid userId, UpdateUserDto dto, CancellationToken cancellationToken = default);

    /// <summary>Kullanıcıyı pasif hale getirir (soft delete).</summary>
    Task<ApiResponse<bool>> DeactivateAsync(Guid userId, CancellationToken cancellationToken = default);
}
