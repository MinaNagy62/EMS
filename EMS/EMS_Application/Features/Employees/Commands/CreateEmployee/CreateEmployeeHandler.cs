using EMS_Application.DTO.Employee;
using EMS_Application.Interfaces;
using EMS_Application.Mapping;
using EMS_Domain.Entities;
using MediatR;

namespace EMS_Application.Features.Employees.Commands.CreateEmployee;

public class CreateEmployeeHandler : IRequestHandler<CreateEmployeeCommand, EmployeeResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateEmployeeHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<EmployeeResponse> Handle(
        CreateEmployeeCommand request,
        CancellationToken cancellationToken)
    {
        var employee = new Employee
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            DateOfBirth = request.DateOfBirth,
            HireDate = request.HireDate,
            Salary = request.Salary,
            Gender = request.Gender,
            JobTitle = request.JobTitle,
            DepartmentId = request.DepartmentId,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Employees.AddAsync(employee);
        await _unitOfWork.SaveChangesAsync();

        return employee.ToResponse();
    }
}
