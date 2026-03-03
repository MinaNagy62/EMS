using EMS_Application.DTO.Employee;
using EMS_Application.Exceptions;
using EMS_Application.Interfaces;
using EMS_Application.Interfaces.Employees;
using EMS_Application.Mapping;

namespace EMS_Application.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IUnitOfWork _unitOfWork;

    public EmployeeService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<EmployeeResponse>> GetAllEmployeesAsync()
    {
        var employees = await _unitOfWork.Employees.GetAllAsync(
            e => e.IsActive,
            e => e.Department);

        return employees.ToResponse();
    }

    public async Task<EmployeeResponse> GetEmployeeByIdAsync(int id)
    {
        var employee = await _unitOfWork.Employees.FindAsync(
            e => e.Id == id && e.IsActive,
            e => e.Department);

        if (employee is null)
            throw new NotFoundException(nameof(employee), id);

        return employee.ToResponse();
    }

    public async Task<EmployeeResponse> CreateEmployeeAsync(CreateEmployeeRequest request)
    {
        var employee = request.ToEntity();
        employee.CreatedAt = DateTime.UtcNow;

        await _unitOfWork.Employees.AddAsync(employee);
        await _unitOfWork.SaveChangesAsync();

        return employee.ToResponse();
    }

    public async Task<EmployeeResponse> UpdateEmployeeAsync(int id, UpdateEmployeeRequest request)
    {
        var existing = await _unitOfWork.Employees.FindAsync(e => e.Id == id && e.IsActive);

        if (existing is null)
            throw new NotFoundException(nameof(existing), id);

        existing.ApplyUpdate(request);

        _unitOfWork.Employees.Update(existing);
        await _unitOfWork.SaveChangesAsync();

        return existing.ToResponse();
    }

    public async Task DeleteEmployeeAsync(int id)
    {
        var existing = await _unitOfWork.Employees.FindAsync(e => e.Id == id && e.IsActive);

        if (existing is null)
            throw new NotFoundException(nameof(existing), id);

        existing.IsActive = false;
        existing.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Employees.Update(existing);
        await _unitOfWork.SaveChangesAsync();
    }
}
