using EMS_Application.Common;
using EMS_Application.DTO.Department;

namespace EMS_Application.Interfaces.Departments;

public interface IDepartmentService
{
    Task<PagedResponse<DepartmentResponse>> GetAllDepartmentsAsync(PagedRequest request);
    Task<DepartmentResponse> GetDepartmentByIdAsync(int id);
    Task<DepartmentResponse> CreateDepartmentAsync(CreateDepartmentRequest request);
    Task<DepartmentResponse> UpdateDepartmentAsync(int id, UpdateDepartmentRequest request);
    Task DeleteDepartmentAsync(int id);
}
