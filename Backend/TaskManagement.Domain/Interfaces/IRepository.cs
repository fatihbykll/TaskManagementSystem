using System.Linq.Expressions;

namespace TaskManagement.Domain.Interfaces;

/// <summary>
/// Persistence mekanizmasını Domain katmanından soyutlar.
/// Domain, EF Core veya herhangi bir ORM'ye doğrudan bağımlı kalmaz;
/// implementasyon Infrastructure'da değiştirilebilir.
/// </summary>
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task AddAsync(T entity, CancellationToken cancellationToken = default);
    void Update(T entity);
    void Delete(T entity);

    /// <summary>
    /// Ham IQueryable döner; servis katmanı LINQ filtrelerini DB'ye SQL olarak göndermek için kullanır.
    /// EF Core-specifik materializasyon (CountAsync, ToListAsync) Infrastructure'da kalır.
    /// </summary>
    IQueryable<T> Query();

    /// <summary>
    /// Dışarıdan oluşturulmuş IQueryable üzerinde sayfalama uygular ve sonucu materializes eder.
    /// Skip/Take ve Count DB'de çalışır; belleğe yalnızca istenen sayfa yüklenir.
    /// </summary>
    Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
        IQueryable<T> query,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
}
