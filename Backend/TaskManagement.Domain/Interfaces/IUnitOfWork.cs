namespace TaskManagement.Domain.Interfaces;

/// <summary>
/// Birden fazla repository üzerindeki değişiklikleri atomik transaction olarak
/// commit etmeyi zorunlu kılar; partial update senaryolarını önler.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IRepository<T> Repository<T>() where T : class;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
