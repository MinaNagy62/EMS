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

    public static IEnumerable<EmployeeResponse> ToResponse(this IEnumerable<Employee> employees)
    {
        return employees.Select(e => e.ToResponse());
    }

    public static Employee ToEntity(this CreateEmployeeRequest request)
    {
        return new Employee
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
            DepartmentId = request.DepartmentId
        };
    }

    public static void ApplyUpdate(this Employee employee, UpdateEmployeeRequest request)
    {
        employee.FirstName = request.FirstName;
        employee.LastName = request.LastName;
        employee.Email = request.Email;
        employee.Phone = request.Phone;
        employee.DateOfBirth = request.DateOfBirth;
        employee.HireDate = request.HireDate;
        employee.Salary = request.Salary;
        employee.Gender = request.Gender;
        employee.JobTitle = request.JobTitle;
        employee.DepartmentId = request.DepartmentId;
        employee.UpdatedAt = DateTime.UtcNow;
    }
}
