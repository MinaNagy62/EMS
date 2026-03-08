using System.Linq.Expressions;
using EMS_Application.Common;
using EMS_Application.DTO.Department;
using EMS_Application.Interfaces;
using EMS_Application.Mapping;
using EMS_Domain.Entities;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

namespace EMS_Application.Features.Departments.Queries.GetAllDepartments;

public class GetAllDepartmentsHandler : IRequestHandler<GetAllDepartmentsQuery, PagedResponse<DepartmentResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMemoryCache _cache;

    private const string CacheKeyPrefix = "departments_";
    private static readonly TimeSpan SlidingExpiration = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan AbsoluteExpiration = TimeSpan.FromHours(1);
    private static CancellationTokenSource _cacheResetToken = new();

    public GetAllDepartmentsHandler(IUnitOfWork unitOfWork, IMemoryCache cache)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task<PagedResponse<DepartmentResponse>> Handle(
        GetAllDepartmentsQuery request,
        CancellationToken cancellationToken)
    {
        var cacheKey = BuildCacheKey(request);

        if (_cache.TryGetValue(cacheKey, out PagedResponse<DepartmentResponse>? cached))
            return cached!;

        var filters = new List<Expression<Func<Department, bool>>>
        {
            d => d.IsActive == (request.IsActive ?? true)
        };

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

    private static string BuildCacheKey(GetAllDepartmentsQuery request)
    {
        return $"{CacheKeyPrefix}p{request.PageNumber}_s{request.PageSize}_search{request.Search}_active{request.IsActive}_sort{request.SortBy}_desc{request.SortDescending}";
    }

    public static void InvalidateCache()
    {
        _cacheResetToken.Cancel();
        _cacheResetToken.Dispose();
        _cacheResetToken = new CancellationTokenSource();
    }
}
