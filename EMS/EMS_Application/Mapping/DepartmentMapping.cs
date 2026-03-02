using EMS_Application.DTO.Department;
using EMS_Domain.Entities;

namespace EMS_Application.Mapping;

public static class DepartmentMapping
{
    public static DepartmentResponse ToResponse(this Department department)
    {
        return new DepartmentResponse
        {
            Id = department.Id,
            Name = department.Name,
            Code = department.Code,
            Description = department.Description,
            IsActive = department.IsActive,
            CreatedAt = department.CreatedAt,
            UpdatedAt = department.UpdatedAt
        };
    }

    public static IEnumerable<DepartmentResponse> ToResponse(this IEnumerable<Department> departments)
    {
        return departments.Select(d => d.ToResponse());
    }

    public static Department ToEntity(this CreateDepartmentRequest request)
    {
        return new Department
        {
            Name = request.Name,
            Code = request.Code,
            Description = request.Description
        };
    }

    public static void ApplyUpdate(this Department department, UpdateDepartmentRequest request)
    {
        department.Name = request.Name;
        department.Code = request.Code;
        department.Description = request.Description;
        department.UpdatedAt = DateTime.UtcNow;
    }
}
