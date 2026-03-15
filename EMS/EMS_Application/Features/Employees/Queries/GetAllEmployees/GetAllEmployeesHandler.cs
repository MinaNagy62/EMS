using System.Linq.Expressions;
using EMS_Application.Common;
using EMS_Application.DTO.Employee;
using EMS_Application.Interfaces;
using EMS_Application.Mapping;
using EMS_Domain.Entities;
using MediatR;

namespace EMS_Application.Features.Employees.Queries.GetAllEmployees;

public class GetAllEmployeesHandler : IRequestHandler<GetAllEmployeesQuery, PagedResponse<EmployeeResponse>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllEmployeesHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResponse<EmployeeResponse>> Handle(
        GetAllEmployeesQuery request,
        CancellationToken cancellationToken)
    {
        var filters = new List<Expression<Func<Employee, bool>>>
        {
            e => e.IsActive
        };

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            filters.Add(e => e.FirstName.ToLower().Contains(search)
                           || e.LastName.ToLower().Contains(search)
                           || e.Email.ToLower().Contains(search));
        }

        if (request.DepartmentId.HasValue)
            filters.Add(e => e.DepartmentId == request.DepartmentId.Value);

        if (request.Gender.HasValue)
            filters.Add(e => e.Gender == request.Gender.Value);

        var pagedEmployees = await _unitOfWork.Employees.GetPagedAsync(
            request, filters, e => e.Department);

        return new PagedResponse<EmployeeResponse>
        {
            Items = pagedEmployees.Items.Select(e => e.ToResponse()).ToList(),
            PageNumber = pagedEmployees.PageNumber,
            PageSize = pagedEmployees.PageSize,
            TotalCount = pagedEmployees.TotalCount
        };
    }
}
