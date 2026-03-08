using EMS_Application.Common;
using EMS_Application.DTO.Department;
using MediatR;

namespace EMS_Application.Features.Departments.Queries.GetAllDepartments;

public class GetAllDepartmentsQuery : PagedRequest, IRequest<PagedResponse<DepartmentResponse>>
{
    public string? Search { get; set; }
    public bool? IsActive { get; set; }
}
