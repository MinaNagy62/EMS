using EMS_Application.DTO.Employee;

namespace EMS_Application.Interfaces.Employees;

public interface IEmployeeService
{
    Task<IEnumerable<EmployeeResponse>> GetAllEmployeesAsync();
    Task<EmployeeResponse?> GetEmployeeByIdAsync(int id);
    Task<EmployeeResponse> CreateEmployeeAsync(CreateEmployeeRequest request);
    Task<EmployeeResponse> UpdateEmployeeAsync(int id, UpdateEmployeeRequest request);
    Task DeleteEmployeeAsync(int id);
}
