using AutoMapper;
using BCrypt.Net;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Interfaces;
using TaskManagement.Application.Wrappers;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Interfaces;

namespace TaskManagement.Application.Services;

/// <summary>
/// Kullanıcı kimlik doğrulama ve profil yönetimi iş mantığı.
/// </summary>
public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UserService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<UserDto>> RegisterAsync(
        CreateUserDto dto, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<User>();

        // DB unique constraint öncesinde fail-fast; kullanıcıya daha hızlı hata dönülür.
        var emailExists = await repo.AnyAsync(u => u.Email == dto.Email, cancellationToken);
        if (emailExists)
            return ApiResponse<UserDto>.FailResult("Bu e-posta adresi zaten kullanımda.");

        var usernameExists = await repo.AnyAsync(u => u.Username == dto.Username, cancellationToken);
        if (usernameExists)
            return ApiResponse<UserDto>.FailResult("Bu kullanıcı adı zaten kullanımda.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = dto.Username,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            // BCrypt adaptive hashing; work factor sayesinde brute-force maliyeti üstel artar.
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true
        };

        await repo.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<UserDto>.SuccessResult(
            _mapper.Map<UserDto>(user), "Kullanıcı başarıyla oluşturuldu.");
    }

    public async Task<ApiResponse<UserDto>> LoginAsync(
        LoginDto dto, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<User>();
        var user = await repo.FirstOrDefaultAsync(u => u.Email == dto.Email, cancellationToken);

        // Timing-safe response: kullanıcı varlığı expose edilmez; user enumeration saldırıları engellenir.
        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return ApiResponse<UserDto>.FailResult("E-posta veya şifre hatalı.");

        if (!user.IsActive)
            return ApiResponse<UserDto>.FailResult("Hesabınız devre dışı bırakılmış.");

        return ApiResponse<UserDto>.SuccessResult(_mapper.Map<UserDto>(user), "Giriş başarılı.");
    }

    public async Task<ApiResponse<UserDto>> GetProfileAsync(
        Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId, cancellationToken);

        if (user == null)
            return ApiResponse<UserDto>.FailResult("Kullanıcı bulunamadı.");

        return ApiResponse<UserDto>.SuccessResult(_mapper.Map<UserDto>(user));
    }

    public async Task<ApiResponse<UserDto>> UpdateProfileAsync(
        Guid userId, UpdateUserDto dto, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<User>();
        var user = await repo.GetByIdAsync(userId, cancellationToken);

        if (user == null)
            return ApiResponse<UserDto>.FailResult("Kullanıcı bulunamadı.");

        // Mevcut email ile aynıysa DB sorgusu yapılmaz; sadece gerçek değişiklik doğrulanır.
        if (!string.IsNullOrWhiteSpace(dto.Email) && dto.Email != user.Email)
        {
            var emailExists = await repo.AnyAsync(
                u => u.Email == dto.Email && u.Id != userId, cancellationToken);

            if (emailExists)
                return ApiResponse<UserDto>.FailResult("Bu e-posta adresi zaten kullanımda.");

            user.Email = dto.Email;
        }

        if (!string.IsNullOrWhiteSpace(dto.FirstName)) user.FirstName = dto.FirstName;
        if (!string.IsNullOrWhiteSpace(dto.LastName)) user.LastName = dto.LastName;
        user.UpdatedAt = DateTime.UtcNow;

        repo.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<UserDto>.SuccessResult(
            _mapper.Map<UserDto>(user), "Profil başarıyla güncellendi.");
    }

    public async Task<ApiResponse<bool>> DeactivateAsync(
        Guid userId, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<User>();
        var user = await repo.GetByIdAsync(userId, cancellationToken);

        if (user == null)
            return ApiResponse<bool>.FailResult("Kullanıcı bulunamadı.");

        // Soft delete: kullanıcıya ait veriler korunur; hard delete yerine IsActive=false tercih edilir.
        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;

        repo.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.SuccessResult(true, "Hesap devre dışı bırakıldı.");
    }
}
