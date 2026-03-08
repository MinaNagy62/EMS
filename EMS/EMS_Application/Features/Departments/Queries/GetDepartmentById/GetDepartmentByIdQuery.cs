using EMS_Application.DTO.Department;
using MediatR;

namespace EMS_Application.Features.Departments.Queries.GetDepartmentById;

public class GetDepartmentByIdQuery : IRequest<DepartmentResponse>
{
    public int Id { get; set; }
}
