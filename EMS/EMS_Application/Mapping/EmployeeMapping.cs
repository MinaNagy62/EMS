using EMS_Application.DTO.Employee;
using EMS_Domain.Entities;

namespace EMS_Application.Mapping;

public static class EmployeeMapping
{
    public static EmployeeResponse ToResponse(this Employee employee)
    {
        return new EmployeeResponse
        {
            Id = employee.Id,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Email = employee.Email,
            Phone = employee.Phone,
            DateOfBirth = employee.DateOfBirth,
            HireDate = employee.HireDate,
            Salary = employee.Salary,
            Gender = employee.Gender,
            JobTitle = employee.JobTitle,
            DepartmentId = employee.DepartmentId,
            DepartmentName = employee.Department?.Name ?? string.Empty,
            IsActive = employee.IsActive,
            CreatedAt = employee.CreatedAt,
            UpdatedAt = employee.UpdatedAt
        };
    }
}
