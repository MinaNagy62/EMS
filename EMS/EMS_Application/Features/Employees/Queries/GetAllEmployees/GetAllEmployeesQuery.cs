using EMS_Application.Common;
using EMS_Application.DTO.Employee;
using EMS_Domain.Enum;
using MediatR;

namespace EMS_Application.Features.Employees.Queries.GetAllEmployees;

public class GetAllEmployeesQuery : PagedRequest, IRequest<PagedResponse<EmployeeResponse>>
{
    public string? Search { get; set; }
    public int? DepartmentId { get; set; }
    public Gender? Gender { get; set; }
}
