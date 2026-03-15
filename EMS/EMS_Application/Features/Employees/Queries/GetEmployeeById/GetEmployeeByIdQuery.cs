using EMS_Application.DTO.Employee;
using MediatR;

namespace EMS_Application.Features.Employees.Queries.GetEmployeeById;

public class GetEmployeeByIdQuery : IRequest<EmployeeResponse>
{
    public int Id { get; set; }
}
