namespace TaskManagement.Domain.Enums;
/// <summary>
/// Kullanıcı yetki seviyeleri.
/// User: standart erişim (kendi task/kategori/ekler).
/// Admin: tüm kullanıcı verilerine okuma + sistem istatistikleri.
/// </summary>
public enum UserRole
{
    User  = 0,
    Admin = 1
}
