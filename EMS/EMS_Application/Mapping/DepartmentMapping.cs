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
}
