using EMS_Application.DTO.Department;
using MediatR;

namespace EMS_Application.Features.Departments.Commands.UpdateDepartment;

public class UpdateDepartmentCommand : IRequest<DepartmentResponse>
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Code { get; set; }
    public string? Description { get; set; }
}
