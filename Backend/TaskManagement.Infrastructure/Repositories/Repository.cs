using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Domain.Interfaces;
using TaskManagement.Infrastructure.Data;

namespace TaskManagement.Infrastructure.Repositories;

/// <summary>
/// Provider-agnostic generic repository.
/// Read-only sorgularda AsNoTracking() ile ChangeTracker overhead'i sıfırlanır;
/// yazma operasyonlarında explicit Update/Delete attach mekanizması yeterlidir.
/// </summary>
public class Repository<T> : IRepository<T> where T : class
{
    private readonly ApplicationDbContext _context;
    private readonly DbSet<T> _dbSet;

    public Repository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    /// <summary>
    /// FindAsync identity cache'i kullanır; PK lookup için en verimli yöntemdir.
    /// Yazma senaryolarında tracking gerekebileceğinden AsNoTracking uygulanmaz.
    /// </summary>
    public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _dbSet.FindAsync(new object[] { id }, cancellationToken);

    /// <summary>Tüm kayıtları read-only olarak getirir. ChangeTracker bypass edilir.</summary>
    public async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _dbSet.AsNoTracking().ToListAsync(cancellationToken);

    /// <summary>Filtrelenmiş kayıtları read-only olarak getirir. ChangeTracker bypass edilir.</summary>
    public async Task<IEnumerable<T>> FindAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
        => await _dbSet.AsNoTracking().Where(predicate).ToListAsync(cancellationToken);

    /// <summary>
    /// Tek kayıt okuma. AsNoTracking ile tracking maliyeti kaldırılır.
    /// Update/Delete için servis katmanı ardından explicit Update(entity) çağırır; EF Core entity'yi attach eder.
    /// </summary>
    public async Task<T?> FirstOrDefaultAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
        => await _dbSet.AsNoTracking().FirstOrDefaultAsync(predicate, cancellationToken);

    public async Task<bool> AnyAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
        => await _dbSet.AnyAsync(predicate, cancellationToken);

    public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
        => await _dbSet.AddAsync(entity, cancellationToken);

    /// <summary>
    /// Untracked entity'yi Modified state'e alır; AsNoTracking ile okunan entity'lerde de çalışır.
    /// </summary>
    public void Update(T entity)
        => _dbSet.Update(entity);

    public void Delete(T entity)
        => _dbSet.Remove(entity);

    /// <summary>
    /// AsNoTracking pipeline başlangıcı. Servis katmanı LINQ filtrelerini DB'ye SQL olarak gönderir.
    /// </summary>
    public IQueryable<T> Query()
        => _dbSet.AsNoTracking().AsQueryable();

    /// <summary>
    /// Dışarıdan oluşturulmuş IQueryable üzerinde sayfalama uygular.
    /// Count ve liste aynı IQueryable'dan türetilir; N+1 sorgu önlenir.
    /// </summary>
    public async Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
        IQueryable<T> query,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}
