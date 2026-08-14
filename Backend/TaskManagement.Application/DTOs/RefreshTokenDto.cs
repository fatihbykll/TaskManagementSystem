using System.ComponentModel.DataAnnotations;
namespace TaskManagement.Application.DTOs;
public class RefreshTokenDto
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
public class ForgotPasswordDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
}
public class ResetPasswordDto
{
    [Required]
    public string Token { get; set; } = string.Empty;
    [Required, MinLength(8)]
    public string NewPassword { get; set; } = string.Empty;
}
