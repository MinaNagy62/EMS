using MediatR;

namespace EMS_Application.Features.Departments.Commands.DeleteDepartment;

public class DeleteDepartmentCommand : IRequest
{
    public int Id { get; set; }
}
