using EMS_Application.DTO.Department;
using EMS_Application.Exceptions;
using EMS_Application.Interfaces;
using EMS_Application.Interfaces.Departments;
using EMS_Application.Mapping;

namespace EMS_Application.Services;

public class DepartmentService : IDepartmentService
{
    private readonly IUnitOfWork _unitOfWork;

    public DepartmentService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<DepartmentResponse>> GetAllDepartmentsAsync()
    {
        var departments = await _unitOfWork.Departments.GetAllAsync(d => d.IsActive);
        return departments.ToResponse();
    }

    public async Task<DepartmentResponse> GetDepartmentByIdAsync(int id)
    {
        var department = await _unitOfWork.Departments.FindAsync(
            d => d.Id == id && d.IsActive);

        if (department is null)
            throw new NotFoundException(nameof(department), id);

        return department.ToResponse();
    }

    public async Task<DepartmentResponse> CreateDepartmentAsync(CreateDepartmentRequest request)
    {
        var department = request.ToEntity();
        department.CreatedAt = DateTime.UtcNow;

        await _unitOfWork.Departments.AddAsync(department);
        await _unitOfWork.SaveChangesAsync();

        return department.ToResponse();
    }

    public async Task<DepartmentResponse> UpdateDepartmentAsync(int id, UpdateDepartmentRequest request)
    {
        var existing = await _unitOfWork.Departments.FindAsync(d => d.Id == id && d.IsActive);

        if (existing is null)
            throw new NotFoundException(nameof(existing), id);

        existing.ApplyUpdate(request);

        _unitOfWork.Departments.Update(existing);
        await _unitOfWork.SaveChangesAsync();

        return existing.ToResponse();
    }

    public async Task DeleteDepartmentAsync(int id)
    {
        var existing = await _unitOfWork.Departments.FindAsync(d => d.Id == id && d.IsActive);

        if (existing is null)
            throw new NotFoundException(nameof(existing), id);

        existing.IsActive = false;
        existing.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Departments.Update(existing);
        await _unitOfWork.SaveChangesAsync();
    }
}
