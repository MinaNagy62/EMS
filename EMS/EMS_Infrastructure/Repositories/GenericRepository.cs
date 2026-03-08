using System.Linq.Expressions;
using EMS_Application.Common;
using EMS_Application.Interfaces;
using EMS_Domain.Entities;
using EMS_Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EMS_Infrastructure.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public GenericRepository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }

    public async Task<IEnumerable<T>> GetAllAsync(
        Expression<Func<T, bool>>? filter = null,
        params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _dbSet;

        if (filter is not null)
            query = query.Where(filter);

        foreach (var include in includes)
            query = query.Include(include);

        return await query.ToListAsync();
    }

    public async Task<PagedResponse<T>> GetPagedAsync(
        PagedRequest request,
        List<Expression<Func<T, bool>>>? filters = null,
        params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _dbSet;

        if (filters is not null)
        {
            foreach (var filter in filters)
                query = query.Where(filter);
        }

        foreach (var include in includes)
            query = query.Include(include);

        var totalCount = await query.CountAsync();

        query = ApplySorting(query, request.SortBy, request.SortDescending);

        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        return new PagedResponse<T>
        {
            Items = items,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }

    private static IQueryable<T> ApplySorting(IQueryable<T> query, string? sortBy, bool sortDescending)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
            return sortDescending ? query.OrderByDescending(x => x.Id) : query.OrderBy(x => x.Id);

        // Validate that the property exists on T (case-insensitive)
        var property = typeof(T).GetProperty(
            sortBy, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

        if (property is null)
            return sortDescending ? query.OrderByDescending(x => x.Id) : query.OrderBy(x => x.Id);

        // Build expression tree: x => x.PropertyName
        var parameter = Expression.Parameter(typeof(T), "x");
        var propertyAccess = Expression.Property(parameter, property);
        var converted = Expression.Convert(propertyAccess, typeof(object));
        var lambda = Expression.Lambda<Func<T, object>>(converted, parameter);

        return sortDescending ? query.OrderByDescending(lambda) : query.OrderBy(lambda);
    }

    public async Task<T?> FindAsync(
        Expression<Func<T, bool>> predicate,
        params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = _dbSet;

        foreach (var include in includes)
            query = query.Include(include);

        return await query.FirstOrDefaultAsync(predicate);
    }

    public async Task AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
    }

    public void Update(T entity)
    {
        _dbSet.Update(entity);
    }

    public void Remove(T entity)
    {
        _dbSet.Remove(entity);
    }
}
