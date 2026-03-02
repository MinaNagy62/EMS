using System.Linq.Expressions;

namespace EMS_Application.Interfaces;

public interface IGenericRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);

    Task<IEnumerable<T>> GetAllAsync(
        Expression<Func<T, bool>>? filter = null,
        params Expression<Func<T, object>>[] includes);

    Task<T?> FindAsync(
        Expression<Func<T, bool>> predicate,
        params Expression<Func<T, object>>[] includes);

    Task AddAsync(T entity);
    void Update(T entity);
    void Remove(T entity);
}
