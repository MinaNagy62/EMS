using EMS_Application.DTO.Employee;
using EMS_Domain.Enum;
using MediatR;

namespace EMS_Application.Features.Employees.Commands.UpdateEmployee;

public class UpdateEmployeeCommand : IRequest<EmployeeResponse>
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string? Phone { get; set; }
    public DateTime DateOfBirth { get; set; }
    public DateTime HireDate { get; set; }
    public decimal Salary { get; set; }
    public Gender Gender { get; set; }
    public string JobTitle { get; set; }
    public int DepartmentId { get; set; }
}
