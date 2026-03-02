using EMS_Application.DTO.Department;

namespace EMS_Application.Interfaces.Departments;

public interface IDepartmentService
{
    Task<IEnumerable<DepartmentResponse>> GetAllDepartmentsAsync();
    Task<DepartmentResponse?> GetDepartmentByIdAsync(int id);
    Task<DepartmentResponse> CreateDepartmentAsync(CreateDepartmentRequest request);
    Task<DepartmentResponse> UpdateDepartmentAsync(int id, UpdateDepartmentRequest request);
    Task DeleteDepartmentAsync(int id);
}
