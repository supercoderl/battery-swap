using System.Linq.Expressions;
using BatterySwap.Domain.Common;

namespace BatterySwap.Application.Common.Interfaces;

/// <summary>Generic repository abstraction over an aggregate root.</summary>
public interface IRepository<T> where T : BaseEntity
{
    IQueryable<T> Query(bool asNoTracking = true);
    Task<T?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<List<T>> ListAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken ct = default);
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken ct = default);
    Task AddAsync(T entity, CancellationToken ct = default);
    void Update(T entity);
    void Remove(T entity);
}
