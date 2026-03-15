using EMS_Application.DTO.Department;
using MediatR;

namespace EMS_Application.Features.Departments.Commands.CreateDepartment;

public class CreateDepartmentCommand : IRequest<DepartmentResponse>
{
    public string Name { get; set; }
    public string Code { get; set; }
    public string? Description { get; set; }
}
