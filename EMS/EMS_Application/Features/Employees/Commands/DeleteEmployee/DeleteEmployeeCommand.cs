using MediatR;

namespace EMS_Application.Features.Employees.Commands.DeleteEmployee;

public class DeleteEmployeeCommand : IRequest
{
    public int Id { get; set; }
}
