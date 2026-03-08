using System.Linq.Expressions;
using EMS_Application.Common;
using EMS_Application.DTO.Department;
using EMS_Application.Exceptions;
using EMS_Application.Interfaces;
using EMS_Application.Interfaces.Departments;
using EMS_Application.Mapping;
using EMS_Domain.Entities;
using FluentValidation;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

namespace EMS_Application.Services;

public class DepartmentService : IDepartmentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<CreateDepartmentRequest> _createValidator;
    private readonly IValidator<UpdateDepartmentRequest> _updateValidator;
    private readonly IMemoryCache _cache;

    private const string CacheKeyPrefix = "departments_";
    private static readonly TimeSpan SlidingExpiration = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan AbsoluteExpiration = TimeSpan.FromHours(1);

    // Shared CancellationTokenSource — when cancelled, ALL department cache entries are evicted
    private static CancellationTokenSource _cacheResetToken = new();

    public DepartmentService(
        IUnitOfWork unitOfWork,
        IValidator<CreateDepartmentRequest> createValidator,
        IValidator<UpdateDepartmentRequest> updateValidator,
        IMemoryCache cache)
    {
        _unitOfWork = unitOfWork;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _cache = cache;
    }

    public async Task<PagedResponse<DepartmentResponse>> GetAllDepartmentsAsync(DepartmentQueryRequest request)
    {
        var cacheKey = BuildCacheKey(request);

        if (_cache.TryGetValue(cacheKey, out PagedResponse<DepartmentResponse>? cached))
            return cached!;

        var filters = new List<Expression<Func<Department, bool>>>();

        // IsActive filter — default to active-only if not specified
        filters.Add(d => d.IsActive == (request.IsActive ?? true));

        // Search filter — searches Name and Code
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            filters.Add(d => d.Name.ToLower().Contains(search)
                           || d.Code.ToLower().Contains(search));
        }

        var pagedDepartments = await _unitOfWork.Departments.GetPagedAsync(request, filters);

        var result = new PagedResponse<DepartmentResponse>
        {
            Items = pagedDepartments.Items.Select(d => d.ToResponse()).ToList(),
            PageNumber = pagedDepartments.PageNumber,
            PageSize = pagedDepartments.PageSize,
            TotalCount = pagedDepartments.TotalCount
        };

        var cacheOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(SlidingExpiration)
            .SetAbsoluteExpiration(AbsoluteExpiration)
            .AddExpirationToken(new CancellationChangeToken(_cacheResetToken.Token));

        _cache.Set(cacheKey, result, cacheOptions);

        return result;
    }

    public async Task<DepartmentResponse> GetDepartmentByIdAsync(int id)
    {
        var department = await _unitOfWork.Departments.FindAsync(
            d => d.Id == id && d.IsActive);

        if (department is null)
            throw new NotFoundException("Department", id);

        return department.ToResponse();
    }

    public async Task<DepartmentResponse> CreateDepartmentAsync(CreateDepartmentRequest request)
    {
        var validationResult = await _createValidator.ValidateAsync(request);

        if (!validationResult.IsValid)
            throw new Exceptions.ValidationException(validationResult.ToErrorDictionary());

        var department = request.ToEntity();
        department.CreatedAt = DateTime.UtcNow;

        await _unitOfWork.Departments.AddAsync(department);
        await _unitOfWork.SaveChangesAsync();

        InvalidateCache();

        return department.ToResponse();
    }

    public async Task<DepartmentResponse> UpdateDepartmentAsync(int id, UpdateDepartmentRequest request)
    {
        var validationResult = await _updateValidator.ValidateAsync(request);

        if (!validationResult.IsValid)
            throw new Exceptions.ValidationException(validationResult.ToErrorDictionary());

        var existing = await _unitOfWork.Departments.FindAsync(d => d.Id == id && d.IsActive);

        if (existing is null)
            throw new NotFoundException("Department", id);

        existing.ApplyUpdate(request);

        _unitOfWork.Departments.Update(existing);
        await _unitOfWork.SaveChangesAsync();

        InvalidateCache();

        return existing.ToResponse();
    }

    public async Task DeleteDepartmentAsync(int id)
    {
        var existing = await _unitOfWork.Departments.FindAsync(d => d.Id == id && d.IsActive);

        if (existing is null)
            throw new NotFoundException("Department", id);

        existing.IsActive = false;
        existing.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Departments.Update(existing);
        await _unitOfWork.SaveChangesAsync();

        InvalidateCache();
    }

    private static string BuildCacheKey(DepartmentQueryRequest request)
    {
        return $"{CacheKeyPrefix}p{request.PageNumber}_s{request.PageSize}_search{request.Search}_active{request.IsActive}_sort{request.SortBy}_desc{request.SortDescending}";
    }

    private static void InvalidateCache()
    {
        _cacheResetToken.Cancel();
        _cacheResetToken.Dispose();
        _cacheResetToken = new CancellationTokenSource();
    }
}
