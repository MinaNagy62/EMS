using EMS_Application.DTO.Employee;
using EMS_Application.Exceptions;
using EMS_Application.Interfaces;
using EMS_Application.Mapping;
using MediatR;

namespace EMS_Application.Features.Employees.Commands.UpdateEmployee;

public class UpdateEmployeeHandler : IRequestHandler<UpdateEmployeeCommand, EmployeeResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateEmployeeHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<EmployeeResponse> Handle(
        UpdateEmployeeCommand request,
        CancellationToken cancellationToken)
    {
        var existing = await _unitOfWork.Employees.FindAsync(
            e => e.Id == request.Id && e.IsActive,
            e => e.Department);

        if (existing is null)
            throw new NotFoundException("Employee", request.Id);

        existing.FirstName = request.FirstName;
        existing.LastName = request.LastName;
        existing.Email = request.Email;
        existing.Phone = request.Phone;
        existing.DateOfBirth = request.DateOfBirth;
        existing.HireDate = request.HireDate;
        existing.Salary = request.Salary;
        existing.Gender = request.Gender;
        existing.JobTitle = request.JobTitle;
        existing.DepartmentId = request.DepartmentId;
        existing.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Employees.Update(existing);
        await _unitOfWork.SaveChangesAsync();

        return existing.ToResponse();
    }
}
