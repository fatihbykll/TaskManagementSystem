using AutoMapper;
using Microsoft.Extensions.Options;
using TaskManagement.Application.Settings;
using TaskManagement.Domain.Enums;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Interfaces;
using TaskManagement.Application.Wrappers;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Interfaces;
namespace TaskManagement.Application.Services;
public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly AppSettings _appSettings;
    private readonly IJwtService _jwtService;
    private readonly IEmailService _emailService;
    public UserService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IOptions<AppSettings> appSettings,
        IJwtService jwtService,
        IEmailService emailService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _appSettings = appSettings.Value;
        _jwtService = jwtService;
        _emailService = emailService;
    }
    public async Task<ApiResponse<UserDto>> RegisterAsync(
        CreateUserDto dto, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<User>();
        if (await repo.AnyAsync(u => u.Email == dto.Email, cancellationToken))
            return ApiResponse<UserDto>.FailResult("Bu e-posta adresi zaten kullanımda.");
        if (await repo.AnyAsync(u => u.Username == dto.Username, cancellationToken))
            return ApiResponse<UserDto>.FailResult("Bu kullanıcı adı zaten kullanımda.");
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = dto.Username,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true,
            Role = dto.Email.Equals(_appSettings.AdminEmail ?? "", StringComparison.OrdinalIgnoreCase)
                   ? UserRole.Admin : UserRole.User
        };
        await repo.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse<UserDto>.SuccessResult(_mapper.Map<UserDto>(user), "Kullanıcı başarıyla oluşturuldu.");
    }
    public async Task<ApiResponse<TokenDto>> LoginAsync(
        LoginDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Repository<User>()
            .FirstOrDefaultAsync(u => u.Email == dto.Email, cancellationToken);
        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return ApiResponse<TokenDto>.FailResult("E-posta veya şifre hatalı.");
        if (!user.IsActive)
            return ApiResponse<TokenDto>.FailResult("Hesabınız devre dışı bırakılmış.");
        // Son giriş zamanını güncelle
        user.LastLoginAt = DateTime.UtcNow;
        _unitOfWork.Repository<User>().Update(user);
        // Access token üret
        var userDto = _mapper.Map<UserDto>(user);
        var token = _jwtService.GenerateToken(userDto);
        // Refresh token oluştur ve kaydet
        var refreshTokenValue = _jwtService.GenerateRefreshToken();
        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = refreshTokenValue,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        };
        await _unitOfWork.Repository<RefreshToken>().AddAsync(refreshToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        token.RefreshToken = refreshTokenValue;
        token.RefreshTokenExpiresAt = refreshToken.ExpiresAt;
        return ApiResponse<TokenDto>.SuccessResult(token, "Giriş başarılı.");
    }
    public async Task<ApiResponse<TokenDto>> RefreshTokenAsync(
        string refreshToken, CancellationToken cancellationToken = default)
    {
        var tokenEntity = await _unitOfWork.Repository<RefreshToken>()
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken, cancellationToken);
        if (tokenEntity == null || !tokenEntity.IsActive)
            return ApiResponse<TokenDto>.FailResult("Refresh token geçersiz veya süresi dolmuş.");
        var user = await _unitOfWork.Repository<User>()
            .GetByIdAsync(tokenEntity.UserId, cancellationToken);
        if (user == null || !user.IsActive)
            return ApiResponse<TokenDto>.FailResult("Kullanıcı bulunamadı.");
        // Eski token'ı iptal et (Rotation)
        tokenEntity.IsRevoked = true;
        tokenEntity.RevokedAt = DateTime.UtcNow;
        _unitOfWork.Repository<RefreshToken>().Update(tokenEntity);
        // Yeni token çifti üret
        var userDto = _mapper.Map<UserDto>(user);
        var newToken = _jwtService.GenerateToken(userDto);
        var newRefreshValue = _jwtService.GenerateRefreshToken();
        var newRefreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = newRefreshValue,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        };
        await _unitOfWork.Repository<RefreshToken>().AddAsync(newRefreshToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        newToken.RefreshToken = newRefreshValue;
        newToken.RefreshTokenExpiresAt = newRefreshToken.ExpiresAt;
        return ApiResponse<TokenDto>.SuccessResult(newToken, "Token yenilendi.");
    }
    public async Task<ApiResponse<bool>> LogoutAsync(
        string refreshToken, CancellationToken cancellationToken = default)
    {
        var tokenEntity = await _unitOfWork.Repository<RefreshToken>()
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken, cancellationToken);
        if (tokenEntity == null)
            return ApiResponse<bool>.SuccessResult(true, "Çıkış yapıldı.");
        tokenEntity.IsRevoked = true;
        tokenEntity.RevokedAt = DateTime.UtcNow;
        _unitOfWork.Repository<RefreshToken>().Update(tokenEntity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse<bool>.SuccessResult(true, "Başarıyla çıkış yapıldı.");
    }
    public async Task<ApiResponse<bool>> ForgotPasswordAsync(
        string email, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Repository<User>()
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
        // Güvenlik: kullanıcı yoksa bile başarılı döndür (user enumeration engeli)
        if (user == null)
            return ApiResponse<bool>.SuccessResult(true, "Şifre sıfırlama bağlantısı e-posta adresinize gönderildi.");
        var resetToken = new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = _jwtService.GenerateRefreshToken(),
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            CreatedAt = DateTime.UtcNow
        };
        await _unitOfWork.Repository<PasswordResetToken>().AddAsync(resetToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _emailService.SendReminderAsync(
            user.Email,
            "Şifre Sıfırlama",
            $"Şifre sıfırlama token'ınız: {resetToken.Token} (1 saat geçerli)",
            cancellationToken);
        return ApiResponse<bool>.SuccessResult(true, "Şifre sıfırlama bağlantısı e-posta adresinize gönderildi.");
    }
    public async Task<ApiResponse<bool>> ResetPasswordAsync(
        ResetPasswordDto dto, CancellationToken cancellationToken = default)
    {
        var tokenEntity = await _unitOfWork.Repository<PasswordResetToken>()
            .FirstOrDefaultAsync(t => t.Token == dto.Token, cancellationToken);
        if (tokenEntity == null || !tokenEntity.IsValid)
            return ApiResponse<bool>.FailResult("Token geçersiz veya süresi dolmuş.");
        var user = await _unitOfWork.Repository<User>()
            .GetByIdAsync(tokenEntity.UserId, cancellationToken);
        if (user == null)
            return ApiResponse<bool>.FailResult("Kullanıcı bulunamadı.");
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;
        tokenEntity.IsUsed = true;
        _unitOfWork.Repository<User>().Update(user);
        _unitOfWork.Repository<PasswordResetToken>().Update(tokenEntity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse<bool>.SuccessResult(true, "Şifre başarıyla sıfırlandı.");
    }
    public async Task<ApiResponse<UserDto>> GetProfileAsync(
        Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId, cancellationToken);
        if (user == null) return ApiResponse<UserDto>.FailResult("Kullanıcı bulunamadı.");
        return ApiResponse<UserDto>.SuccessResult(_mapper.Map<UserDto>(user));
    }
    public async Task<ApiResponse<UserDto>> UpdateProfileAsync(
        Guid userId, UpdateUserDto dto, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<User>();
        var user = await repo.GetByIdAsync(userId, cancellationToken);
        if (user == null) return ApiResponse<UserDto>.FailResult("Kullanıcı bulunamadı.");
        if (!string.IsNullOrWhiteSpace(dto.Email) && dto.Email != user.Email)
        {
            if (await repo.AnyAsync(u => u.Email == dto.Email && u.Id != userId, cancellationToken))
                return ApiResponse<UserDto>.FailResult("Bu e-posta adresi zaten kullanımda.");
            user.Email = dto.Email;
        }
        if (!string.IsNullOrWhiteSpace(dto.FirstName)) user.FirstName = dto.FirstName;
        if (!string.IsNullOrWhiteSpace(dto.LastName)) user.LastName = dto.LastName;
        user.UpdatedAt = DateTime.UtcNow;
        repo.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse<UserDto>.SuccessResult(_mapper.Map<UserDto>(user), "Profil başarıyla güncellendi.");
    }
    public async Task<ApiResponse<bool>> DeactivateAsync(
        Guid userId, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<User>();
        var user = await repo.GetByIdAsync(userId, cancellationToken);
        if (user == null) return ApiResponse<bool>.FailResult("Kullanıcı bulunamadı.");
        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;
        repo.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse<bool>.SuccessResult(true, "Hesap devre dışı bırakıldı.");
    }
}
